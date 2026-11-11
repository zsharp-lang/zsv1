namespace ZSharp.Compiler
{
    public interface ICompileIRStatement
    {
        public IResult<LIR.Statement, Error> CompileIRStatement(object? target);
    }
}
