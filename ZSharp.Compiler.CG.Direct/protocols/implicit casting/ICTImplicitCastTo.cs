namespace ZSharp.Compiler
{
    public interface ICTImplicitCastTo
    {
        public Result ImplicitCast(Compiler compiler, CompilerObject type);
    }
}
