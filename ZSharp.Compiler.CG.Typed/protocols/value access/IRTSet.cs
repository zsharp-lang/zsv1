namespace ZSharp.Compiler
{
    public interface IRTSet
    {
        public IResult Set(Compiler compiler, CompilerObject @object, CompilerObject value);
    }
}
