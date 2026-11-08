namespace ZSharp.Importer.ILLoader.Objects
{
    /// <summary>
    /// <c>null</c>, as C# has it: it has no type, and goes to any type that can be null,
    /// a reference type or a <c>Nullable</c>, where it is that type's null.
    /// </summary>
    internal sealed class Null(ILLoader loader)
        : CompilerObject
        , ICTImplicitCastTo
        , ICTCastTo
    {
        public IResult ImplicitCast(CompilerObject type)
            => type.Is<IILType>(out var known) && known.GetILType() is { } target
            && (!target.IsValueType || Nullable.GetUnderlyingType(target) is not null)
                ? Result.Ok(new Literal(null, target, loader))
                : Result.Error(new UnsupportedError("null only goes to a type that can be null."));

        public IResult Cast(CompilerObject type)
            => ImplicitCast(type);
    }
}
