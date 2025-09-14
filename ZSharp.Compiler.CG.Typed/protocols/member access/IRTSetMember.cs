namespace ZSharp.Compiler
{
    public interface IRTSetMember<M>
    {
        public Result Member(Compiler compiler, CompilerObject @object, M member, CompilerObject value);
    }
}
