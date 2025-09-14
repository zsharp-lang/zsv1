namespace ZSharp.Compiler
{
    public interface IRTImplicitCastTo
    {
        public Result ImplicitCast(Compiler compiler, CompilerObject @object, CompilerObject type);
    }
}
