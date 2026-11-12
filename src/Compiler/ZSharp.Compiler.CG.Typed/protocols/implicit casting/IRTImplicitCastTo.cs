namespace ZSharp.Compiler
{
    public interface IRTImplicitCastTo
    {
        public IResult ImplicitCast(CompilerObject @object, CompilerObject type);
    }
}
