using ZSharp.Objects;

namespace ZSharp.Runtime.NET
{
    internal sealed class TypeObject(Type il, IR.IType ir, CompilerObject co)
        : ICompileTime
    {
        public Type IL { get; } = il;

        public IR.IType IR { get; } = ir;

        public CompilerObject CO { get; } = co;

        public CompilerObject GetCO()
            => CO;
    }
}
