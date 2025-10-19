namespace ZSharp.Compiler
{
    public interface IRTGetMember<M>
    {
        public IResult Member(Compiler compiler, CompilerObject @object, M member);
    }
}
