namespace ZSharp.Compiler
{
    public interface IDynamicallyTyped
        : CompilerObject
        , IHasRuntimeDescriptor
    {
        CompilerObject IHasRuntimeDescriptor.GetRuntimeDescriptor(Compiler compiler)
            => GetType(compiler);

        public IType GetType(Compiler compiler);
    }
}
