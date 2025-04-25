namespace ZSharp.Compiler
{
    public interface ITyped : IDynamicallyTyped
    {
        public IType Type { get; }

        IType IDynamicallyTyped.GetType(Compiler compiler)
            => Type;
    }
}
