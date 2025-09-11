namespace ZSharp.Compiler
{
    public interface ICTSetMember<M>
    {
        public Result Member(Compiler compiler, M member, CompilerObject value);
    }
}
