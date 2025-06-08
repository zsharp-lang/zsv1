namespace ZSharp.Compiler
{
    public interface ICTCallable_NEW
        : CompilerObject
    {
        public CompilerObjectResult Call(Compiler compiler, Argument_NEW<CompilerObject>[] arguments);
    }
}
