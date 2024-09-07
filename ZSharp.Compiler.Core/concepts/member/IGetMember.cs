namespace ZSharp.Compiler
{
    public interface ICTGetMember<M>
    {
        public CGObject Member(ZSCompiler compiler, M member);
    }

    public interface IRTGetMember<M>
    {
        public CGObject Member(ZSCompiler compiler, CGObject value, M member);
    }
}
