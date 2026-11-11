namespace ZSharp.Compiler
{
    public interface ICompileIRType
    {
        public IResult<LIR.Type, Error> CompileIRType(object? target);
    }
}
