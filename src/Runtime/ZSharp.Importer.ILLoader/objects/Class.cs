using System.Reflection;

namespace ZSharp.Importer.ILLoader.Objects
{
    /// <summary>
    /// A CLR type. It discovers members when asked, so loading a type never reaches
    /// the types its members mention.
    /// </summary>
    internal sealed class Class(ILLoader loader, Type il)
        : CompilerObject
        , ICompileIRType
        , ICTCallable
        , ICTCastTo
        , ICTGetMember<MemberName>
        , IRTGetMember<MemberName>
    {
        public Type IL { get; } = il;

        public IResult<LIR.Type, Error> CompileIRType(object? target)
            => Result<LIR.Type>.Ok(loader.Handle(IL));

        /// <summary>
        /// Calling a type constructs an instance of it.
        /// </summary>
        public IResult Call(Argument[] arguments)
            => new MethodGroup(loader, IL.GetConstructors(), null).Call(arguments);

        /// <summary>
        /// Casting a type to what describes it is reflection.
        /// </summary>
        public IResult Cast(CompilerObject targetType)
        {
            var node = loader.Load(IL);

            return targetType.Is<Class>(out var target) && target.IL.IsInstanceOfType(node)
                ? Result.Ok(new Literal(node, node.GetType(), loader))
                : Result.Error(new UnsupportedError($"{IL.Name} can't be cast to that type."));
        }

        public IResult Member(MemberName member)
            => Find(BindingFlags.Static, member, null);

        public IResult Member(CompilerObject @object, MemberName member)
            => Find(BindingFlags.Instance, member, @object);

        private IResult Find(BindingFlags kind, MemberName member, CompilerObject? receiver)
        {
            var flags = BindingFlags.Public | BindingFlags.FlattenHierarchy | kind;

            var methods = IL
                .GetMethods(flags)
                .Where(method => !method.IsSpecialName && Named(method, member))
                .ToArray<MethodBase>();

            if (methods.Length > 0)
                return Result.Ok(new MethodGroup(loader, methods, receiver));

            if (IL.GetProperties(flags).FirstOrDefault(property => Named(property, member)) is { } found)
                return Result.Ok(new Accessor(
                    receiver,
                    () => loader.Class(found.PropertyType),
                    found.GetMethod is { IsPublic: true } getter ? loader.Handle(getter) : null,
                    found.SetMethod is { IsPublic: true } setter ? loader.Handle(setter) : null
                ));

            if (IL.GetFields(flags).FirstOrDefault(field => Named(field, member)) is { } field)
                return Result.Ok(new Accessor(
                    receiver,
                    () => loader.Class(field.FieldType),
                    loader.Handle(field, isSet: false),
                    field.IsInitOnly ? null : loader.Handle(field, isSet: true)
                ));

            return Result.Error(new UnsupportedError($"{IL.Name} has no member named {member}."));
        }

        private static bool Named(MemberInfo member, string name)
            => (member.GetCustomAttribute<AliasAttribute>()?.Name ?? member.Name) == name;
    }
}
