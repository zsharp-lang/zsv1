namespace ZSharp.Compiler
{
    public interface ICTImplicitCastTo
        : CompilerObject
    {
        public CompilerObjectResult ImplicitCast(Compiler compiler, IType type);
    }
}
