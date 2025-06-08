namespace ZSharp.Compiler
{
    public interface IHasRuntimeDescriptor
        : CompilerObject
    {
        public CompilerObject GetRuntimeDescriptor(Compiler compiler);
    }
}
