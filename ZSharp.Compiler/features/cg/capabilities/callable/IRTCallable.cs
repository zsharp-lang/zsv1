namespace ZSharp.Compiler
{
    public interface IRTCallable_NEW
        : CompilerObject
    {
        public CompilerObjectResult Call(Compiler compiler, CompilerObject @object, Argument_NEW<CompilerObject>[] arguments);
    }
}
