namespace ZSharp.Compiler
{
    public interface ICTGetMember<M>
    {
        public CompilerObject Member(Compiler compiler, M member);
    }

    public interface IRTGetMember<M>
    {
        public CompilerObject Member(Compiler compiler, CompilerObject value, M member);
    }
}
