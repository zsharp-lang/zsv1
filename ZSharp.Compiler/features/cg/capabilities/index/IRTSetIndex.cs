namespace ZSharp.Compiler
{
    public interface IRTSetIndex_NEW
        : CompilerObject
    {
        public CompilerObjectResult Index(Compiler compiler, CompilerObject @object, Argument_NEW<CompilerObject>[] arguments, CompilerObject value);
    }
}
