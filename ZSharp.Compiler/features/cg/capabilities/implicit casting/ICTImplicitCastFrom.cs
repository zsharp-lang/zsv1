namespace ZSharp.Compiler
{
    public interface ICTImplicitCastFrom
        : CompilerObject
    {
        public CompilerObjectResult ImplicitCast(Compiler compiler, CompilerObject value);
    }
}
