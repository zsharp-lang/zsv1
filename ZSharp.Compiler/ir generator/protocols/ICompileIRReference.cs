namespace ZSharp.Compiler
{
    public interface ICompileIRReference<out T>
    {
        public T CompileIRReference(Compiler compiler);
    }
}
