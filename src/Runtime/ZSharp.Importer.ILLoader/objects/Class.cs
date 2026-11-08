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
        , IRTCallable
        , IRTImplicitCastTo
        , IILType
        , IAssignableTo
        , ICompileIRExpression
        , ITyped
    {
        public Type IL { get; } = il;

        /// <summary>
        /// A class used as a value is its type object.
        /// </summary>
        CompilerObject ITyped.Type => loader.Class(typeof(Type));

        public IResult<LIR.Expression, Error> CompileIRExpression(object? target)
            => Result<LIR.Expression>.Ok(new LIR.Call(loader.TypeOf(loader.Handle(IL)), []));

        public Type GetILType() => IL;

        /// <summary>
        /// A CLR type only goes to other CLR types.
        /// </summary>
        public bool IsAssignableTo(CompilerObject target)
            => target.Is<IILType>(out var type) && type.GetILType().IsAssignableFrom(IL);

        public IResult<LIR.Type, Error> CompileIRType(object? target)
            => Result<LIR.Type>.Ok(loader.Handle(IL));

        /// <summary>
        /// Calling a type constructs an instance of it.
        /// </summary>
        public IResult Call(Argument[] arguments)
            => Core.Compiler.ModuleScope.CG.Call(
                OverloadGroup.Of(IL.GetConstructors().Select(constructor => new Method(loader, constructor, null))),
                arguments
            );

        /// <summary>
        /// A value of a delegate type is called through its <c>Invoke</c> method, as in C#.
        /// </summary>
        public IResult Call(CompilerObject @object, Argument[] arguments)
            => typeof(Delegate).IsAssignableFrom(IL) && IL.GetMethod("Invoke") is { } invoke
                ? new Method(loader, invoke, @object).Call(arguments)
                : Result.Error(new UnsupportedError($"A value of {IL.Name} can't be called."));

        /// <summary>
        /// Casting a type to what describes it is reflection.
        /// </summary>
        public IResult Cast(CompilerObject targetType)
        {
            var node = loader.Load(IL);

            return Core.Compiler.ModuleScope.TS.IsAssignableTo(targetType, loader.Class(node.GetType()))
                ? Result.Ok(new Literal(node, node.GetType(), loader))
                : Result.Error(new UnsupportedError($"{IL.Name} can't be cast to that type."));
        }

        /// <summary>
        /// A value of a value type becomes an object by boxing it, when it goes to a type
        /// it is assignable to and that isn't its own. Anything else that is assignable
        /// stays as it is.
        /// </summary>
        public IResult ImplicitCast(CompilerObject @object, CompilerObject type)
            => IL.IsValueType
            && Core.Compiler.ModuleScope.TS.IsAssignableTo(type, this)
            && !Core.Compiler.ModuleScope.TS.IsAssignableTo(this, type)
                ? Result.Ok(new Call(loader.Box(IL), [@object], () => type))
                : Result.Error(new UnsupportedError($"{IL.Name} has no conversion to that type."));

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
                return Result.Ok(OverloadGroup.Of(methods.Select(method => new Method(loader, method, receiver))));

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
