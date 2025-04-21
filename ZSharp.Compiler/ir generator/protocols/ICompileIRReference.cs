namespace ZSharp.Compiler
{
    public interface ICompileIRReference<T>
    {
        public T CompileIRReference(Compiler compiler);
    }
}
