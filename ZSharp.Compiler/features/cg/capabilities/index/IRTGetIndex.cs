namespace ZSharp.Compiler
{
    public interface IRTGetIndex_NEW
        : CompilerObject
    {
        public CompilerObjectResult Index(Compiler compiler, CompilerObject @object, Argument_NEW<CompilerObject>[] arguments);
    }
}
