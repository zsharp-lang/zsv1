using System.Reflection;

namespace ZSharp.Importer.ILLoader.Objects
{
    /// <summary>
    /// A generic CLR type given types that aren't CLR types yet, as a class being loaded
    /// with the code that uses it: <c>List[Item]</c> inside the module that defines
    /// <c>Item</c>. It lowers to an LIR instance of the definition, and its members are the
    /// definition's, with its type arguments in place of the definition's type parameters,
    /// called on that instance. The platform makes the instance and finds its members.
    /// </summary>
    internal sealed class Instantiation(ILLoader loader, Type definition, CompilerObject[] arguments)
        : CompilerObject
        , ICompileIRType
        , ICTCallable
        , ICTGetMember<MemberName>
        , IRTGetMember<MemberName>
        , IRTGetIndex
        , IRTSetIndex
        , IAssignableTo
        , ICTImplicitCastTo
        , ICTCastTo
    {
        /// <summary>
        /// An instance goes where an HIR type is expected as what describes it: the
        /// definition's description given what describes each argument. Casting it to what
        /// describes it is reflection, as for any type.
        /// </summary>
        public IResult ImplicitCast(CompilerObject targetType)
            => loader.Describes(targetType) ? Cast(targetType) : Result.Error(new UnsupportedError($"{definition.Name} isn't a description."));

        public IResult Cast(CompilerObject targetType)
            => Core.Compiler.ModuleScope.TS.IsAssignableTo(targetType, loader.Class(typeof(Package.DotNet.GenericInstance)))
                && loader.DescribeInstance(definition, arguments) is { } described
                ? described
                : Result.Error(new UnsupportedError($"{definition.Name} can't be cast to that type."));

        private LIR.GenericInstance? lowered;

        public Type Definition => definition;

        public IReadOnlyList<CompilerObject> Arguments => arguments;

        public LIR.GenericInstance Lowered()
            => lowered ??= new LIR.GenericInstance(
                loader.Handle(definition),
                arguments.Select(argument => Lower(argument))
            );

        private LIR.Type Lower(CompilerObject type)
            => Core.Compiler.ModuleScope.IR.CompileType(type, loader.Runtime).Unwrap();

        /// <summary>
        /// A method or constructor of the definition, called on this instance: it takes and
        /// gives the types this instance has, and an instance method takes the object first.
        /// </summary>
        public LIR.InstanceMember Lowered(MethodBase member)
        {
            var signature = new LIR.Signature();

            if (member is MethodInfo { IsStatic: false })
                signature.Parameters.Add(new(Lowered()) { Name = "this" });

            foreach (var parameter in member.GetParameters())
                signature.Parameters.Add(new(Lower(TypeOf(parameter.ParameterType))) { Name = parameter.Name });

            signature.ReturnType = member switch
            {
                ConstructorInfo => Lowered(),
                MethodInfo { ReturnType: var type } when type != typeof(void) => Lower(TypeOf(type)),
                _ => null
            };

            return new LIR.InstanceMember(Lowered(), loader.Handle(member), signature);
        }

        public IResult<LIR.Type, Error> CompileIRType(object? target)
            => Result<LIR.Type>.Ok(Lowered());

        /// <summary>
        /// A type as a member of this instantiation has it: a type parameter of the
        /// definition is the argument given for it.
        /// </summary>
        public CompilerObject TypeOf(Type type)
            => loader.Substitute(type, parameter => parameter.DeclaringMethod is null ? arguments[parameter.GenericParameterPosition] : null);

        /// <summary>
        /// Another instantiation of the same definition over the same types is this type,
        /// and every instantiation of a class is an object.
        /// </summary>
        public bool IsAssignableTo(CompilerObject target)
        {
            if (target is Instantiation other && other.Definition == definition && other.Arguments.Count == arguments.Length)
                return arguments.Zip(other.Arguments).All(pair =>
                    Core.Compiler.ModuleScope.TS.IsAssignableTo(pair.First, pair.Second)
                    && Core.Compiler.ModuleScope.TS.IsAssignableTo(pair.Second, pair.First));

            return target.Is<IILType>(out var known) && known.GetILType() == typeof(object);
        }

        public IResult Call(Argument[] arguments)
            => Core.Compiler.ModuleScope.CG.Call(
                OverloadGroup.Of(definition.GetConstructors().Select(constructor => new InstantiationMember(loader, this, constructor, null))),
                arguments
            );

        public IResult Member(MemberName member)
            => Find(BindingFlags.Static, member, null);

        public IResult Member(CompilerObject @object, MemberName member)
            => Find(BindingFlags.Instance, member, @object);

        private IResult Find(BindingFlags kind, MemberName member, CompilerObject? receiver)
        {
            var flags = BindingFlags.Public | kind;

            var methods = definition.GetMethods(flags)
                .Where(method => !method.IsSpecialName && !method.IsGenericMethodDefinition && method.Name == member)
                .ToArray();

            if (methods.Length > 0)
                return Result.Ok(OverloadGroup.Of(methods.Select(method => new InstantiationMember(loader, this, method, receiver))));

            if (definition.GetProperties(flags).FirstOrDefault(property => property.Name == member && property.GetIndexParameters().Length == 0) is { } property)
                return Result.Ok(new Accessor(
                    receiver,
                    () => TypeOf(property.PropertyType),
                    property.GetMethod is { IsPublic: true } getter ? Lowered(getter) : null,
                    property.SetMethod is { IsPublic: true } setter ? Lowered(setter) : null
                ));

            return Result.Error(new UnsupportedError($"{definition.Name} has no member named {member}."));
        }

        public IResult Index(CompilerObject @object, Argument[] arguments)
        {
            var getters = Indexers().Where(indexer => indexer.GetMethod is { IsPublic: true })
                .Select(indexer => (CompilerObject)new InstantiationMember(loader, this, indexer.GetMethod!, @object)).ToArray();

            if (getters.Length == 0)
                return Result.Error(new UnsupportedError($"{definition.Name} has no indexer that can be read."));

            if (Core.Compiler.ModuleScope.CG.Call(OverloadGroup.Of(getters), arguments).When(out var read).Error(out var error))
                return Result.Error(error);

            return Result.Ok(new Indexed(read!, value => Index(@object, arguments, value)));
        }

        public IResult Index(CompilerObject @object, Argument[] arguments, CompilerObject value)
        {
            var setters = Indexers().Where(indexer => indexer.SetMethod is { IsPublic: true })
                .Select(indexer => (CompilerObject)new InstantiationMember(loader, this, indexer.SetMethod!, @object)).ToArray();

            return setters.Length == 0
                ? Result.Error($"The indexer of {definition.Name} can't be assigned.")
                : Core.Compiler.ModuleScope.CG.Call(OverloadGroup.Of(setters), [.. arguments, new Argument(value)]);
        }

        private PropertyInfo[] Indexers()
            => [.. definition.GetProperties(BindingFlags.Public | BindingFlags.Instance).Where(property => property.GetIndexParameters().Length > 0)];
    }

    /// <summary>
    /// A method or constructor of an instantiation, which takes and gives types as the
    /// instantiation has them.
    /// </summary>
    internal sealed class InstantiationMember(ILLoader loader, Instantiation owner, MethodBase member, CompilerObject? receiver)
        : CompilerObject
        , ICTCallable
    {
        public IResult Call(Argument[] arguments)
        {
            var parameters = member.GetParameters().Select(parameter => (CompilerObject)new InstantiationParameter(owner, parameter, loader));

            if (Arguments.Match(parameters, arguments) is not { } values)
                return Result.Error($"{member.Name} can't be called with these arguments.");

            Func<CompilerObject>? returnType = member switch
            {
                ConstructorInfo => () => owner,
                MethodInfo { ReturnType: var type } when type != typeof(void) => () => owner.TypeOf(type),
                _ => null
            };

            return Result.Ok(new Call(
                owner.Lowered(member),
                [.. (receiver is null ? [] : new[] { receiver }), .. values],
                returnType
            ));
        }
    }

    internal sealed class InstantiationParameter(Instantiation owner, ParameterInfo parameter, ILLoader loader)
        : CompilerObject
        , ICTMatchParameter
    {
        public IResult Match(ArgumentStream arguments)
            => Arguments.Take(
                parameter.Name ?? string.Empty,
                owner.TypeOf(parameter.ParameterType),
                parameter.HasDefaultValue ? () => new Literal(parameter.DefaultValue, parameter.ParameterType, loader) : null,
                arguments
            );
    }
}
