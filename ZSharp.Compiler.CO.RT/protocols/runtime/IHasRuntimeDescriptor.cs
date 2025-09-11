namespace ZSharp.Compiler
{
    public interface IHasRuntimeDescriptor
    {
        public Result GetRuntimeDescriptor(Compiler compiler);
    }
}
