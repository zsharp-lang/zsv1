namespace ZSharp.Objects
{
    public interface IRTBoundMember
    {
        public CompilerObject Bind(Compiler.Compiler compiler, CompilerObject value);
    }
}
