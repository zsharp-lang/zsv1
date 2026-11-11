namespace ZSharp.Compiler
{
    public interface ICompileIRExpression
    {
        public IResult<LIR.Expression, Error> CompileIRExpression(object? target);
    }
}
