namespace ZSharp.Compiler
{
    public interface ICTGetMember<M>
    {
        public CompilerObjectResult Member(Compiler compiler, M member);
    }
}
