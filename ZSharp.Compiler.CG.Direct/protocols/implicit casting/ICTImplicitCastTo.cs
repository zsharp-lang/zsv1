namespace ZSharp.Compiler
{
    public interface ICTImplicitCastTo
    {
        public IResult ImplicitCast(Compiler compiler, CompilerObject type);
    }
}
