namespace ZSharp.Compiler
{
    public interface IIRTypeCompiler
    {
        public IResult<T, Error> CompileType<T>(CompilerObject @object, object? target)
            where T : class, IType;
    }
}