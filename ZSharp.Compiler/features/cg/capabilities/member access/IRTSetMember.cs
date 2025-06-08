namespace ZSharp.Compiler
{
    public interface IRTSetMember<M>
    {
        public CompilerObjectResult Member(Compiler compiler, CompilerObject @object, M member, CompilerObject value);
    }
}
