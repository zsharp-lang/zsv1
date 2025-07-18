namespace ZSharp.Compiler
{
    public interface ICTCallable
        : CompilerObject
    {
        public CompilerObjectResult Call(Compiler compiler, Argument_NEW<CompilerObject>[] arguments);
    }
}
