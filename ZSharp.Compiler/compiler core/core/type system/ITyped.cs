namespace ZSharp.Compiler
{
    public interface ITyped : IDynamicallyTyped
    {
        public CompilerObject Type { get; }

        CompilerObject IDynamicallyTyped.GetType(Compiler compiler)
            => Type;
    }
}
