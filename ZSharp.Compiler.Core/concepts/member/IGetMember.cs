namespace ZSharp.Compiler
{
    public interface ICTGetMember<M>
    {
        public CTObject Member(ZSCompiler compiler, M member);
    }

    public interface IRTGetMember<M>
    {
        public CTObject Member(ZSCompiler compiler, CTObject value, M member);
    }
}
