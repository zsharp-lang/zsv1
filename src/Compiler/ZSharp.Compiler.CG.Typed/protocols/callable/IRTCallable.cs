namespace ZSharp.Compiler
{
    public interface IRTCallable
    {
        public IResult Call(CompilerObject @object, Argument[] arguments);
    }
}
