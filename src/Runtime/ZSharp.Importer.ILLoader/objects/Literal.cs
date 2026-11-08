namespace ZSharp.Importer.ILLoader.Objects
{
    /// <summary>
    /// A value that already exists, whether written in code or exposed from C#.
    /// </summary>
    internal sealed class Literal(object? value, Type type, ILLoader loader)
        : CompilerObject
        , ICompileIRExpression
        , ITyped
    {
        CompilerObject ITyped.Type => loader.Class(type);

        public IResult<LIR.Expression, Error> CompileIRExpression(object? target)
            => Result<LIR.Expression>.Ok(new LIR.Constant(loader.Handle(type), value));
    }
}
