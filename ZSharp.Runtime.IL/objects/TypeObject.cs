namespace ZSharp.Runtime.NET
{
    public abstract class TypeObject(Type il, IR.IType ir)
    {
        public Type IL { get; } = il;

        public IR.IType IR { get; } = ir;
    }
}
