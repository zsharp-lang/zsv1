namespace ZSharp.Compiler
{
    public interface ICTSetMember<M>
    {
        public IResult Member(Compiler compiler, M member, CompilerObject value);
    }
}
