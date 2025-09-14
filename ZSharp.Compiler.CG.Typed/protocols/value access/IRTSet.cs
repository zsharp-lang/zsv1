namespace ZSharp.Compiler
{
    public interface IRTSet
    {
        public Result Set(Compiler compiler, CompilerObject @object, CompilerObject value);
    }
}
