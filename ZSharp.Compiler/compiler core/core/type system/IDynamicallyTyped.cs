namespace ZSharp.Compiler
{
    public interface IDynamicallyTyped
    {
        public CompilerObject GetType(Compiler compiler);
    }
}
