namespace ZSharp.Compiler
{
    public interface IIRTypeCompiler
    {
        public IResult<T, Error> CompileType<T>(CompilerObject @object)
            where T : class, IType;
    }
}