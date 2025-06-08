namespace ZSharp.Compiler
{
    public interface IRTGetMember<M>
    {
        public CompilerObjectResult Member(Compiler compiler, CompilerObject @object, M member);
    }
}
