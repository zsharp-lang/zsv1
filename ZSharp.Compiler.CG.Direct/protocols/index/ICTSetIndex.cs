namespace ZSharp.Compiler
{
    public interface ICTSetIndex
    {
        public IResult Index(Compiler compiler, Argument[] arguments, CompilerObject value);
    }
}
