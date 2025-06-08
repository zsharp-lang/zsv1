namespace ZSharp.Compiler
{
    public interface ICTSetIndex_NEW
        : CompilerObject
    {
        public CompilerObjectResult Index(Compiler compiler, Argument_NEW<CompilerObject>[] arguments, CompilerObject value);
    }
}
