namespace ZSharp.Compiler
{
    public interface IRTImplicitCastTo
        : CompilerObject
    {
        public CompilerObjectResult ImplicitCast(Compiler compiler, CompilerObject @object, IType type);
    }
}
