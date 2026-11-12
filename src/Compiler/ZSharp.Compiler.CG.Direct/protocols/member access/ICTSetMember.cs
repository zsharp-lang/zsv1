namespace ZSharp.Compiler
{
    public interface ICTSetMember<M>
    {
        public IResult Member(M member, CompilerObject value);
    }
}
