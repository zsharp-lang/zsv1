namespace ZSharp.Compiler
{
    public interface ICTImplicitCastFrom
    {
        public IResult ImplicitCast(Compiler compiler, CompilerObject value);
    }
}
