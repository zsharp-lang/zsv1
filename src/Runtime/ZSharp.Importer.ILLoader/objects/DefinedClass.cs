using ZSharp.LIR.CLR;

using Scope = Core.Compiler.ModuleScope;

namespace ZSharp.Importer.ILLoader.Objects
{
    /// <summary>
    /// A class defined in Z#, of the CLR's meta-type. It is a definition, as a Z# class is:
    /// its members are what its HIR says, and it is lowered for each CLR platform it is
    /// used on, to a class definition of that platform's, which the platform realizes. So
    /// the same class is a class of the running process and of an assembly written for a
    /// version of .NET, and code reaches it through this, whichever it is lowered for.
    /// </summary>
    internal sealed class DefinedClass(ILLoader loader, Package.DotNet.Class node)
        : CompilerObject
        , ICompileIRType
        , ICTCallable
        , ICTGetMember<MemberName>
        , IRTGetMember<MemberName>
        , ICTCastTo
        , ICTImplicitCastTo
        , ICTGetIndex
        , ICompileIRDefinitionIn<LIR.Module>
        , IDefinedClass
        , IAssignableTo
        , ICompileIRExpression
        , ITyped
    {
        public Package.DotNet.Class Node { get; } = node;

        public string Name => Node.Name;

        /// <summary>
        /// What it is lowered to, for each target.
        /// </summary>
        public Dictionary<object, LoweredClass> Lowerings { get; } = [];

        public IResult<LoweredClass, Error> For(object? target)
            => loader.Lower(this, target);

        /// <summary>
        /// A class used as a value is its type object.
        /// </summary>
        CompilerObject ITyped.Type => loader.Class(typeof(Type));

        public IResult<LIR.Expression, Error> CompileIRExpression(object? target)
            => For(target).When(out var lowered).Error(out var error)
                ? Result<LIR.Expression>.Error(error)
                : Result<LIR.Expression>.Ok(new LIR.Call(loader.TypeOf(lowered!.Definition), []));

        /// <summary>
        /// A class held by a module, as a Z# module holds its definitions, is the module's:
        /// the module defines it for the target, which writes it with the module.
        /// </summary>
        public Error? CompileIRDefinition(LIR.Module owner, object? target)
        {
            if (For(target).When(out var lowered).Error(out var error))
                return error;

            if (!owner.Metadata.OfType<DefinesClass>().Any(defined => defined.Definition == lowered!.Definition))
                owner.Metadata.Add(new DefinesClass(lowered!.Definition));

            return null;
        }

        public IResult<LIR.Type, Error> CompileIRType(object? target)
            => For(target).When(out var lowered).Error(out var error)
                ? Result<LIR.Type>.Error(error)
                : Result<LIR.Type>.Ok(lowered!.Definition);

        /// <summary>
        /// Casting a class to what describes it is reflection, as for any type: code that
        /// describes the type it is where the code runs, which is its HIR node in the
        /// process that defined it.
        /// </summary>
        public IResult Cast(CompilerObject targetType)
            => loader.Description(Node) is var described && Scope.TS.IsAssignableTo(targetType, loader.Class(described.GetType()))
                ? loader.Described(this, targetType)
                : Result.Error(new UnsupportedError($"{Name} can't be cast to that type."));

        /// <summary>
        /// A class goes where an HIR type is expected, as in a signature, as what describes
        /// it. Anywhere else it is its type object.
        /// </summary>
        public IResult ImplicitCast(CompilerObject targetType)
            => loader.Describes(targetType) ? Cast(targetType) : Result.Error(new UnsupportedError($"{Name} isn't a description."));

        /// <summary>
        /// A class goes to itself, to the interfaces it implements, and wherever its base
        /// goes.
        /// </summary>
        public bool IsAssignableTo(CompilerObject target)
            => target.Is<IDefinedClass>(out var defined) && defined.Node == Node
            || Node.Interfaces.Any(@interface => Scope.HIRLoader.LoadType(@interface).Ok(out var loaded) && Scope.TS.IsAssignableTo(target, loaded))
            || Base is { } @base && Scope.TS.IsAssignableTo(target, @base);

        /// <summary>
        /// A class with no constructor gets one here, as loading doesn't change HIR.
        /// </summary>
        public List<Package.DotNet.Constructor> Constructors { get; }
            = loader.IsInterface(node) ? [] : node.Constructors.Count == 0 ? [new() { Body = new() }] : node.Constructors;

        /// <summary>
        /// The class this one derives from, or <see langword="null"/> if it can't be loaded.
        /// </summary>
        public CompilerObject? Base
            => Node.Base is null
                ? loader.Class(typeof(object))
                : Scope.HIRLoader.LoadType(Node.Base).Ok(out var loaded) ? loaded : null;

        /// <summary>
        /// A generic class given its type arguments is an instance of it.
        /// </summary>
        public IResult Index(Argument[] arguments)
            => Node.GenericParameters.Count > 0 && arguments.Length == Node.GenericParameters.Count && arguments.All(argument => argument.Name is null)
                ? Result.Ok(new DefinedInstance(loader, this, [.. arguments.Select(argument => argument.Object)]))
                : Result.Error(new UnsupportedError(Node.GenericParameters.Count == 0
                    ? $"{Name} isn't generic, so it takes no type arguments."
                    : $"{Name} takes {Node.GenericParameters.Count} type arguments."));

        public IResult Call(Argument[] arguments)
            => Scope.CG.Call(
                OverloadGroup.Of(Constructors.Select((constructor, index) => new Callable(
                    $"A constructor of {Name}",
                    target => For(target).When(out var lowered).Error(out var error)
                        ? Result<LIR.ICallable>.Error(error)
                        : Result<LIR.ICallable>.Ok(lowered!.Constructors[index]),
                    loader.CallParameters(constructor.Parameters, constructor.CustomMetadata),
                    null,
                    () => this,
                    loader,
                    isMethod: false
                ))),
                arguments
            );

        public IResult Member(MemberName member)
            => Find(member, null);

        public IResult Member(CompilerObject @object, MemberName member)
            => Find(member, @object);

        private IResult Find(MemberName member, CompilerObject? receiver)
        {
            var methods = Node.Methods
                .Where(method => method.Name == member && method.IsStatic == receiver is null)
                .ToArray();

            if (methods.Length > 0)
                return Result.Ok(OverloadGroup.Of(methods.Select(method => Callable(method, receiver, Method(method), null))));

            if (Node.Properties.FirstOrDefault(property => property.Name == member && property.IsStatic == receiver is null) is { } property)
                return Result.Ok(new Accessor(
                    receiver,
                    () => Scope.HIRLoader.LoadType(property.Type!).Unwrap(),
                    property.Getter is { } getter ? Method(getter) : null,
                    property.Setter is { } setter ? Method(setter) : null
                ));

            if (receiver is not null && Node.Fields.FirstOrDefault(field => field.Name == member) is { } field)
                return Result.Ok(new Accessor(
                    receiver,
                    () => Scope.HIRLoader.LoadType(field.Type).Unwrap(),
                    target => For(target).When(out var lowered).Error(out var error)
                        ? Result<LIR.ICallable>.Error(error)
                        : Result<LIR.ICallable>.Ok(FieldAccess(lowered!, field, isSet: false)),
                    target => For(target).When(out var lowered).Error(out var error)
                        ? Result<LIR.ICallable>.Error(error)
                        : Result<LIR.ICallable>.Ok(FieldAccess(lowered!, field, isSet: true))
                ));

            if (Base is not { } @base)
                return Result.Error(new UnsupportedError($"{Name} has no member named {member}."));

            return receiver is null
                ? Scope.CG.Member(@base, member)
                : Scope.CG.Member(new Upcast(receiver, @base), member);
        }

        /// <summary>
        /// A method, as the class is lowered for the target.
        /// </summary>
        public Callee Method(Package.DotNet.Method method)
            => target => For(target).When(out var lowered).Error(out var error)
                ? Result<LIR.ICallable>.Error(error)
                : Result<LIR.ICallable>.Ok(lowered!.Methods[method]);

        /// <summary>
        /// A method reached by its name: one that is generic is given its type arguments
        /// first, and one that isn't is called as it is. Its types are read with what an
        /// instance of the class gives its type parameters, if it is reached on one.
        /// </summary>
        public CompilerObject Callable(Package.DotNet.Method method, CompilerObject? receiver, Callee callee, Substitution? substitution)
            => method.GenericParameters.Count > 0 && (substitution is null || !method.GenericParameters.All(substitution.Given.ContainsKey))
                ? new GenericMethod(loader, this, method, receiver, callee, substitution)
                : new Callable(
                    $"{Name}.{method.Name}",
                    callee,
                    loader.CallParameters(method.Parameters, method.CustomMetadata),
                    receiver,
                    method.ReturnType is { } returnType ? () => Scope.HIRLoader.LoadType(returnType).Unwrap() : null,
                    loader,
                    isMethod: true,
                    substitution
                );

        /// <summary>
        /// Reading or writing a field, as the target lowers the class.
        /// </summary>
        public ClassFieldAccess FieldAccess(LoweredClass lowered, Package.DotNet.Field field, bool isSet)
        {
            var declared = lowered.Fields[field];
            var signature = new LIR.Signature();

            signature.Parameters.Add(new(lowered.Definition));

            if (isSet)
                signature.Parameters.Add(new(declared.Type));
            else
                signature.ReturnType = declared.Type;

            return new ClassFieldAccess(declared, isSet, signature);
        }
    }

    /// <summary>
    /// A defined class as one target has it: its definition, and its members in it.
    /// </summary>
    internal sealed class LoweredClass(ClassDefinition definition)
    {
        public ClassDefinition Definition { get; } = definition;

        public List<ClassConstructor> Constructors { get; } = [];

        public Dictionary<Package.DotNet.Method, ClassMethod> Methods { get; } = [];

        public Dictionary<Package.DotNet.Field, ClassField> Fields { get; } = [];

        public Dictionary<Package.DotNet.GenericParameter, ClassGenericParameter> Parameters { get; } = [];

        /// <summary>
        /// The parameters of each constructor and method, its object first, which its code
        /// is lowered with.
        /// </summary>
        public Dictionary<object, List<LIR.Parameter>> Signatures { get; } = [];
    }

    /// <summary>
    /// A method or constructor of a defined class, optionally bound to the object it is
    /// called on, as each target has it. A method goes where a delegate is expected, as any
    /// CLR method does.
    /// </summary>
    internal sealed class Callable(
        string name,
        Callee callable,
        List<HIR.Code.Parameter> parameters,
        CompilerObject? receiver,
        Func<CompilerObject>? returnType,
        ILLoader loader,
        bool isMethod = true,
        Substitution? substitution = null
    )
        : CompilerObject
        , ICTCallable
        , ISpecific
        , ICTImplicitCastTo
        , ICTCastTo
        , ICompileIRExpression
        , ITyped
    {
        private T Typed<T>(Func<T> typing)
            => substitution is null ? typing() : substitution.In(typing);

        // A method is a value only if it has a delegate type of its own: a constructor, or
        // a method a type in whose signature isn't the CLR's, isn't.
        bool CompilerObject.Is<T>([System.Diagnostics.CodeAnalysis.NotNullWhen(true)] out T? result) where T : class
            => (result = (typeof(T) == typeof(ITyped) || typeof(T) == typeof(ICompileIRExpression)) && Natural() is null ? null : this as T) is not null;

        CompilerObject ITyped.Type => loader.Class(Natural()!);

        /// <summary>
        /// A method used as a value is a delegate of its own signature.
        /// </summary>
        public IResult<LIR.Expression, Error> CompileIRExpression(object? target)
            => new DefinedDelegate(loader, Natural()!, callable, receiver, loader.Class(Natural()!)).CompileIRExpression(target);

        /// <summary>
        /// The delegate type a method is a value of when nothing asks for another: a
        /// <c>Func</c> or an <c>Action</c> of its signature, if its types are the CLR's.
        /// </summary>
        private Type? Natural()
            => Typed(() =>
            {
                if (!isMethod || Arguments.Types(parameters) is not { } types)
                    return null;

                var clr = new List<Type>();

                foreach (var type in types.Append(returnType?.Invoke() ?? loader.Class(typeof(void))))
                    if (type.Is<IILType>(out var known) && known.GetILType() is { IsByRef: false, IsPointer: false } found)
                        clr.Add(found);
                    else
                        return null;

                return System.Linq.Expressions.Expression.GetDelegateType([.. clr]);
            });

        private CompilerObject Returned()
            => Typed(returnType!);

        /// <summary>
        /// A delegate of a type whose parameters go to the method's, and whose result is
        /// where what the method returns goes.
        /// </summary>
        public IResult ImplicitCast(CompilerObject type)
            => Typed(() => Delegated(type));

        private IResult Delegated(CompilerObject type)
        {
            if (!isMethod || !type.Is<IILType>(out var known) || known.GetILType() is not { } delegateType
                || !typeof(Delegate).IsAssignableFrom(delegateType) || delegateType.GetMethod("Invoke") is not { } invoke
                || Arguments.Types(parameters) is not { } types || !Fits(types, invoke))
                return Result.Error(new UnsupportedError($"{name} isn't a delegate of that type."));

            return Result.Ok(new DefinedDelegate(loader, delegateType, callable, receiver, type));
        }

        public IResult Cast(CompilerObject type)
            => ImplicitCast(type);

        private bool Fits(CompilerObject[] types, System.Reflection.MethodInfo invoke)
        {
            var accepted = invoke.GetParameters();

            if (types.Length != accepted.Length
                || !types.Zip(accepted).All(pair => Scope.TS.IsAssignableTo(pair.First, loader.Class(pair.Second.ParameterType))))
                return false;

            return invoke.ReturnType == typeof(void)
                ? returnType is null
                : returnType is not null && Scope.TS.IsAssignableTo(loader.Class(invoke.ReturnType), Returned());
        }

        public IResult Call(Argument[] arguments)
            => Typed(() =>
            {
                if (Arguments.Parameters(parameters) is not { } matching || Arguments.Match(matching, arguments) is not { } values)
                    return Result.Error($"{name} can't be called with these arguments.");

                return Result.Ok(new Call(callable, [.. (receiver is null ? [] : new[] { receiver }), .. values], returnType is null ? null : Returned));
            });

        public bool IsAsSpecificAs(CompilerObject other)
            => Typed(() => Arguments.Types(parameters) is { } types && ZSharp.Compiler.Parameters.AreAsSpecificAs(types, other));
    }

    /// <summary>
    /// A delegate made of a method, bound to the object it was reached through, if any.
    /// </summary>
    internal sealed class BoundDelegate(ILLoader loader, LIR.ICallable make, CompilerObject? receiver, CompilerObject type)
        : CompilerObject
        , ICompileIRExpression
        , ITyped
    {
        CompilerObject ITyped.Type => type;

        public IResult<LIR.Expression, Error> CompileIRExpression(object? target)
        {
            LIR.Expression bound = new LIR.Constant(loader.Handle(typeof(object)), null);

            if (receiver is not null)
                if (Scope.IR.CompileExpression(receiver, target).When(out var compiled).Error(out var error))
                    return Result<LIR.Expression>.Error(error);
                else
                    bound = compiled!;

            return Result<LIR.Expression>.Ok(new LIR.Call(make, [bound]));
        }
    }

    /// <summary>
    /// A delegate made of a method of a defined class, bound to the object it was reached
    /// through, if any, of the method as the target has it.
    /// </summary>
    internal sealed class DefinedDelegate(ILLoader loader, Type delegateType, Callee method, CompilerObject? receiver, CompilerObject type)
        : CompilerObject
        , ICompileIRExpression
        , ITyped
    {
        CompilerObject ITyped.Type => type;

        public IResult<LIR.Expression, Error> CompileIRExpression(object? target)
        {
            if (method(target).When(out var callable).Error(out var error))
                return Result<LIR.Expression>.Error(error);

            if (callable is not ClassMethod declared)
                return Result<LIR.Expression>.Error("Only a method of a class can be made a delegate of.");

            LIR.Expression bound = new LIR.Constant(loader.Handle(typeof(object)), null);

            if (receiver is not null)
                if (Scope.IR.CompileExpression(receiver, target).When(out var compiled).Error(out error))
                    return Result<LIR.Expression>.Error(error);
                else
                    bound = compiled!;

            return Result<LIR.Expression>.Ok(new LIR.Call(loader.Delegate(delegateType, declared), [bound]));
        }
    }
}
