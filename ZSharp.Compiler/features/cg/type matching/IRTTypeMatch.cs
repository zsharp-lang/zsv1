namespace ZSharp.Compiler
{
    public interface IRTTypeMatch
        : CompilerObject
    {
        public Result<TypeMatch, Error> Match(Compiler compiler, CompilerObject value, IType type);
    }
}
