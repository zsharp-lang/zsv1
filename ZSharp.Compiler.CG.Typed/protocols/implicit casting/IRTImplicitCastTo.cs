namespace ZSharp.Compiler
{
    public interface IRTImplicitCastTo
    {
        public IResult ImplicitCast(Compiler compiler, CompilerObject @object, CompilerObject type);
    }
}
