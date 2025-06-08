namespace ZSharp.Compiler
{
    public interface IRTGet
        : CompilerObject
    {
        public CompilerObjectResult Get(Compiler compiler, CompilerObject @object);
    }
}
