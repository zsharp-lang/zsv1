using ZSharp.LIR.CLR;

using Scope = ZSharp.Compiler.Current;

namespace ZSharp.Importer.ILLoader.Objects
{
    /// <summary>
    /// A generic class defined in Z#, given its type arguments. Its members are the
    /// class's, with its arguments in place of its type parameters, called on the instance
    /// of the class each target lowers it to.
    /// </summary>
    internal sealed class DefinedInstance(ILLoader loader, DefinedClass @class, CompilerObject[] arguments)
        : CompilerObject
        , ICompileIRType
        , ICTCallable
        , IRTGetMember<MemberName>
        , ICTCastTo
        , ICTImplicitCastTo
        , IAssignableTo
        , ICompileIRExpression
        , ITyped
    {
        public DefinedClass Class { get; } = @class;

        public CompilerObject[] Arguments { get; } = arguments;

        public Substitution Substitution { get; } = new(loader, @class.Node.GenericParameters.Zip(arguments).ToDictionary(pair => pair.First, pair => pair.Second));

        CompilerObject ITyped.Type => loader.Class(typeof(Type));

        /// <summary>
        /// The instance, as a target lowers it, with how its members' signatures read.
        /// </summary>
        public IResult<LoweredInstance, Error> For(object? target)
        {
            if (Class.For(target).When(out var lowered).Error(out var error))
                return Result<LoweredInstance>.Error(error);

            var given = new Dictionary<LIR.Type, LIR.Type>();
            var types = new List<LIR.Type>();

            foreach (var (parameter, argument) in Class.Node.GenericParameters.Zip(Arguments))
                if (Scope.IR.CompileType(argument, target).When(out var type).Error(out error))
                    return Result<LoweredInstance>.Error(error);
                else
                {
                    given[lowered!.Parameters[parameter]] = type!;
                    types.Add(type!);
                }

            return Result<LoweredInstance>.Ok(new(new LIR.GenericInstance(lowered!.Definition, types), given));
        }

        public IResult<LIR.Type, Error> CompileIRType(object? target)
            => For(target).When(out var lowered).Error(out var error)
                ? Result<LIR.Type>.Error(error)
                : Result<LIR.Type>.Ok(lowered.Instance);

        public IResult<LIR.Expression, Error> CompileIRExpression(object? target)
            => For(target).When(out var lowered).Error(out var error)
                ? Result<LIR.Expression>.Error(error)
                : Result<LIR.Expression>.Ok(new LIR.Call(loader.TypeOf(lowered.Instance), []));

        /// <summary>
        /// Casting an instance to what describes it is reflection: the description of the
        /// class, given what describes each argument.
        /// </summary>
        public IResult Cast(CompilerObject targetType)
            => Scope.TS.IsAssignableTo(targetType, loader.Class(typeof(Package.DotNet.GenericInstance)))
                && loader.DescribeInstance(loader.Description(Class.Node), Arguments) is { } described
                ? described
                : Result.Error(new UnsupportedError($"{Class.Name} can't be cast to that type."));

        public IResult ImplicitCast(CompilerObject targetType)
            => loader.Describes(targetType) ? Cast(targetType) : Result.Error(new UnsupportedError($"{Class.Name} isn't a description."));

        /// <summary>
        /// An instance goes to an instance of the same class whose arguments are its own.
        /// </summary>
        public bool IsAssignableTo(CompilerObject target)
            => target is DefinedInstance other && other.Class == Class
            && other.Arguments.Zip(Arguments).All(pair => Scope.TS.IsAssignableTo(pair.First, pair.Second) && Scope.TS.IsAssignableTo(pair.Second, pair.First));

        /// <summary>
        /// A member of the class, called on the instance, with the signature the instance
        /// gives it.
        /// </summary>
        public Callee On(Func<LoweredClass, LIR.ICallable> member)
            => target =>
            {
                if (For(target).When(out var lowered).Error(out var error))
                    return Result<LIR.ICallable>.Error(error);

                if (Class.For(target).When(out var definition).Error(out error))
                    return Result<LIR.ICallable>.Error(error);

                var declared = member(definition!);

                return Result<LIR.ICallable>.Ok(new LIR.InstanceMember(lowered.Instance, declared, Substitution.Of(declared.Signature, lowered.Given)));
            };

        public IResult Call(Argument[] arguments)
            => Candidates.Call(Class.Constructors.Select((constructor, index) => new Callable(
                    $"A constructor of {Class.Name}",
                    On(lowered => lowered.Constructors[index]),
                    loader.CallParameters(constructor),
                    null,
                    () => this,
                    loader,
                    isMethod: false,
                    Substitution
                )),
                arguments
            );

        public IResult Member(CompilerObject @object, MemberName member)
        {
            var methods = Class.Node.Methods
                .Where(method => method.Name == member && !method.IsStatic)
                .ToArray();

            if (methods.Length > 0)
                return Candidates.Of(methods.Select(method => Class.Callable(method, @object, On(lowered => lowered.Methods[method]), Substitution)));

            if (Class.Node.Fields.FirstOrDefault(field => field.Name == member) is { } field)
                return Result.Ok(new Accessor(
                    @object,
                    () => Substitution.In(() => Scope.HIRLoader.LoadType(field.Type).Unwrap()),
                    On(lowered => Class.FieldAccess(lowered, field, isSet: false)),
                    On(lowered => Class.FieldAccess(lowered, field, isSet: true))
                ));

            return Result.Error(new UnsupportedError($"{Class.Name} has no member named {member}."));
        }
    }

    /// <summary>
    /// An instance of a defined class as a target lowers it, with what it gives the class's
    /// type parameters there.
    /// </summary>
    internal sealed record LoweredInstance(LIR.GenericInstance Instance, Dictionary<LIR.Type, LIR.Type> Given);

    /// <summary>
    /// Type parameters given types: while their members are typed, a type parameter is the
    /// type it is given, and in LIR, the type it is given as a target lowers it.
    /// </summary>
    internal sealed class Substitution(ILLoader loader, Dictionary<Package.DotNet.GenericParameter, CompilerObject> given)
    {
        public Dictionary<Package.DotNet.GenericParameter, CompilerObject> Given { get; } = given;

        public T In<T>(Func<T> typing)
            => loader.Substituting(Given, typing);

        public static LIR.Signature Of(LIR.Signature signature, Dictionary<LIR.Type, LIR.Type> given)
        {
            var result = new LIR.Signature { ReturnType = signature.ReturnType is { } returned ? Of(returned, given) : null };

            foreach (var parameter in signature.Parameters)
                result.Parameters.Add(new(Of(parameter.Type, given)) { Name = parameter.Name });

            return result;
        }

        public static LIR.Type Of(LIR.Type type, Dictionary<LIR.Type, LIR.Type> given)
            => type switch
            {
                _ when given.TryGetValue(type, out var argument) => argument,
                LIR.GenericInstance instance => new LIR.GenericInstance(Of(instance.Definition, given), instance.Arguments.Select(argument => Of(argument, given))),
                ByRefType reference => new ByRefType(Of(reference.Target, given)),
                ClrArrayType array => new ClrArrayType(Of(array.Element, given), array.Rank, array.IsVector),
                ClrPointerType pointer => new ClrPointerType(Of(pointer.Target, given)),
                LIR.Reference reference => new LIR.Reference(Of(reference.Target, given)),
                _ => type
            };
    }

    /// <summary>
    /// A generic method of a defined class, given its type arguments, as each target
    /// instantiates the method it lowers it to.
    /// </summary>
    internal sealed class GenericMethod(ILLoader loader, DefinedClass @class, Package.DotNet.Method method, CompilerObject? receiver, Callee definition, Substitution? outer)
        : CompilerObject
        , ICTGetIndex
    {
        public IResult Index(Argument[] arguments)
        {
            if (arguments.Length != method.GenericParameters.Count || arguments.Any(argument => argument.Name is not null))
                return Result.Error($"{@class.Name}.{method.Name} takes {method.GenericParameters.Count} type arguments.");

            var given = new Dictionary<Package.DotNet.GenericParameter, CompilerObject>(outer?.Given ?? []);

            foreach (var (parameter, argument) in method.GenericParameters.Zip(arguments))
                given[parameter] = argument.Object;

            Callee instantiated = target =>
            {
                if (definition(target).When(out var callable).Error(out var error))
                    return Result<LIR.ICallable>.Error(error);

                if (@class.For(target).When(out var lowered).Error(out error))
                    return Result<LIR.ICallable>.Error(error);

                var types = new Dictionary<LIR.Type, LIR.Type>();
                var listed = new List<LIR.Type>();

                foreach (var (parameter, argument) in method.GenericParameters.Zip(arguments))
                    if (Scope.IR.CompileType(argument.Object, target).When(out var type).Error(out error))
                        return Result<LIR.ICallable>.Error(error);
                    else
                    {
                        types[lowered!.Parameters[parameter]] = type!;
                        listed.Add(type!);
                    }

                return Result<LIR.ICallable>.Ok(new LIR.MethodInstance(callable!, listed, Substitution.Of(callable!.Signature, types)));
            };

            return Result.Ok(@class.Callable(method, receiver, instantiated, new Substitution(loader, given)));
        }
    }
}
