namespace ZSharp.Compiler
{
    public interface IRTGetMember<M>
    {
        public IResult Member(CompilerObject @object, M member);
    }
}
