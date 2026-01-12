namespace ZSharp.Compiler
{
    public interface ICTCallable
    {
        public IResult Call(Compiler compiler, Argument[] arguments);
    }
}
