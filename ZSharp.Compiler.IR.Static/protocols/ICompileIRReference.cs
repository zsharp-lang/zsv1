namespace ZSharp.Compiler
{
    public interface ICompileIRReference<T>
        where T : class
    {
        public Result<T> CompileIRReference(Compiler compiler);
    }
}
