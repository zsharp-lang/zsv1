namespace ZSharp.Compiler
{
    public interface IRTSetIndex
    {
        public Result Index(Compiler compiler, CompilerObject @object, Argument[] arguments, CompilerObject value);
    }
}
