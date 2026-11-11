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

            return type.Is<ICompileIRType>(out _)
                ? Result.Ok(new NullOf(type))
                : Result.Error(new UnsupportedError("null only goes to a type that can be null."));
        }

        public IResult Cast(CompilerObject type)
            => ImplicitCast(type);
    }

    /// <summary>
    /// The null of a type.
    /// </summary>
    internal sealed class NullOf(CompilerObject type)
        : CompilerObject
        , ICompileIRExpression
        , ITyped
    {
        CompilerObject ITyped.Type => type;

        public IResult<LIR.Expression, Error> CompileIRExpression(object? target)
            => ZSharp.Compiler.Current.IR.CompileType(type, target).When(out var lowered).Error(out var error)
                ? Result<LIR.Expression>.Error(error)
                : Result<LIR.Expression>.Ok(new LIR.Constant(lowered!, null));
    }
}
