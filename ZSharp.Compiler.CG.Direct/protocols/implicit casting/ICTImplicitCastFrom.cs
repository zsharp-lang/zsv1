namespace ZSharp.Compiler
{
    public interface ICTImplicitCastFrom
    {
        public Result ImplicitCast(Compiler compiler, CompilerObject value);
    }
}
