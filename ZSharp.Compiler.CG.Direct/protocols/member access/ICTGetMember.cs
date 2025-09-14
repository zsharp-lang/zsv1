namespace ZSharp.Compiler
{
    public interface ICTGetMember<M>
    {
        public Result Member(Compiler compiler, M member);
    }
}
