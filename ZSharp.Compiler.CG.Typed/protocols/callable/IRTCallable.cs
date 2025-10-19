namespace ZSharp.Compiler
{
    public interface IRTCallable
    {
        public IResult Call(Compiler compiler, CompilerObject @object, Argument[] arguments);
    }
}
