namespace ZSharp.Compiler
{
    public interface IIRTypeCompiler
    {
        public Result<T> CompileType<T>(CompilerObject @object)
            where T : class, IType;
    }
}