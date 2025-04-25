namespace ZSharp.Compiler
{
    public interface IDynamicallyTyped
    {
        public IType GetType(Compiler compiler);
    }
}
