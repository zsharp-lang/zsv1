namespace ZSharp.Compiler
{
    public interface IRTSetIndex
    {
        public IResult Index(Compiler compiler, CompilerObject @object, Argument[] arguments, CompilerObject value);
    }
}
