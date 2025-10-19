namespace ZSharp.Compiler
{
    public interface ICTGetMember<M>
    {
        public IResult Member(Compiler compiler, M member);
    }
}
