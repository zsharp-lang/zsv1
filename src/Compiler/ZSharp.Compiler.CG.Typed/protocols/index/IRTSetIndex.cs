namespace ZSharp.Compiler
{
    public interface IRTSetIndex
    {
        public IResult Index(CompilerObject @object, Argument[] arguments, CompilerObject value);
    }
}
