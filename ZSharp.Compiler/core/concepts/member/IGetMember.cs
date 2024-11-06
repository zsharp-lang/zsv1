namespace ZSharp.Compiler
{
    public interface ICTGetMember<M>
    {
        public CGObject Member(Compiler compiler, M member);
    }

    public interface IRTGetMember<M>
    {
        public CGObject Member(Compiler compiler, CGObject value, M member);
    }
}
