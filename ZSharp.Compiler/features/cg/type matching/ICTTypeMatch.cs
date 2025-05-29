namespace ZSharp.Compiler
{
    public interface ICTTypeMatch
        : CompilerObject
    {
        public Result<TypeMatch, Error> Match(Compiler compiler, IType type);
    }
}
