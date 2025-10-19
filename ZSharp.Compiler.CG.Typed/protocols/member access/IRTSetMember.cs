namespace ZSharp.Compiler
{
    public interface IRTSetMember<M>
    {
        public IResult Member(Compiler compiler, CompilerObject @object, M member, CompilerObject value);
    }
}
