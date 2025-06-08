namespace ZSharp.Compiler
{
    public interface ICTSet
        : CompilerObject
    {
        public CompilerObjectResult Set(Compiler compiler, CompilerObject value);
    }
}
