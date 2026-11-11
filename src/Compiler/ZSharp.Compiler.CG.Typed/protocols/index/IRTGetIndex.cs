namespace ZSharp.Compiler
{
    public interface IRTGetIndex
    {
        public IResult Index(CompilerObject @object, Argument[] arguments);
    }
}
