namespace ZSharp.Compiler
{
    public interface IIRReferenceCompiler
    {
        public Result<T> CompileReference<T>(CompilerObject @object)
            where T : class;
    }
}
