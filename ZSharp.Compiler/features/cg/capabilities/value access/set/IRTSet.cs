namespace ZSharp.Compiler
{
    public interface IRTSet
        : CompilerObject
    {
        public CompilerObjectResult Set(Compiler compiler, CompilerObject @object, CompilerObject value);
    }
}
