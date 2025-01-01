namespace ZSharp.Compiler
{
    public interface ITyped
    {
        public CompilerObject Type { get; }

        public CompilerObject GetType(Compiler compiler)
            => Type;
    }
}
