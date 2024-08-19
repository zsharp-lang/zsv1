namespace ZSharp.CTRuntime
{
    public interface ICTGetMember<M>
    {
        public Code Member(ZSCompiler compiler, M member);
    }

    public interface IRTGetMember<M>
    {
        public Code Member(ZSCompiler compiler, Code value, M member);
    }
}
