namespace ZSharp.Compiler
{
    public interface IRTGetMember<M>
    {
        public Result Member(Compiler compiler, CompilerObject @object, M member);
    }
}
