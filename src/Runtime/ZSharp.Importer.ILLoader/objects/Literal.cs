namespace ZSharp.Importer.ILLoader.Objects
{
    internal sealed class Literal(object value, LIR.TypeHandle type)
        : CompilerObject
        , ICompileIRExpression
    {
        public IResult<LIR.Expression, Error> CompileIRExpression(object? target)
            => Result<LIR.Expression>.Ok(new LIR.Constant(type, value));
    }
}
