namespace ZSharp.Compiler
{
    public interface IRTGetIndex
    {
        public Result Index(Compiler compiler, CompilerObject @object, Argument[] arguments);
    }
}
