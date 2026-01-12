namespace ZSharp.Compiler
{
    public interface IRTGetIndex
    {
        public IResult Index(Compiler compiler, CompilerObject @object, Argument[] arguments);
    }
}
