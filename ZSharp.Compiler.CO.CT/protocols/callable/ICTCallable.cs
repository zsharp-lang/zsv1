namespace ZSharp.Compiler
{
    public interface ICTCallable
    {
        public Result Call(Compiler compiler, Argument[] arguments);
    }
}
