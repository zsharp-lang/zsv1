namespace ZSharp.Compiler
{
    public interface IRTSetMember<M>
    {
        public IResult Member(CompilerObject @object, M member, CompilerObject value);
    }
}
