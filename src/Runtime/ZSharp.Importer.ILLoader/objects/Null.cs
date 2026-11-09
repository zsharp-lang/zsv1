namespace ZSharp.Importer.ILLoader.Objects
{
    /// <summary>
    /// <c>null</c>, as C# has it: it has no type, and goes to any type that can be null,
    /// where it is that type's null. A CLR value type can't be null unless it is a
    /// <c>Nullable</c>.
    /// </summary>
    internal sealed class Null(ILLoader loader)
        : CompilerObject
        , ICTImplicitCastTo
        , ICTCastTo
    {
        public IResult ImplicitCast(CompilerObject type)
        {
            if (type.Is<IILType>(out var known) && known.GetILType() is { IsValueType: true } value && Nullable.GetUnderlyingType(value) is null)
                return Result.Error(new UnsupportedError("null only goes to a type that can be null."));

            return Core.Compiler.ModuleScope.IR.CompileType(type, loader.Runtime).Ok(out var lowered)
                ? Result.Ok(new NullOf(lowered, type))
                : Result.Error(new UnsupportedError("null only goes to a type that can be null."));
        }

        public IResult Cast(CompilerObject type)
            => ImplicitCast(type);
    }

    /// <summary>
    /// The null of a type.
    /// </summary>
    internal sealed class NullOf(LIR.Type lowered, CompilerObject type)
        : CompilerObject
        , ICompileIRExpression
        , ITyped
    {
        CompilerObject ITyped.Type => type;

        public IResult<LIR.Expression, Error> CompileIRExpression(object? target)
            => Result<LIR.Expression>.Ok(new LIR.Constant(lowered, null));
    }
}
