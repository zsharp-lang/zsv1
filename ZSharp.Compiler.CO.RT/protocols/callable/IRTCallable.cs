namespace ZSharp.Compiler
{
    public interface IRTCallable
    {
        public Result Call(Compiler compiler, CompilerObject @object, Argument[] arguments);
    }
}
