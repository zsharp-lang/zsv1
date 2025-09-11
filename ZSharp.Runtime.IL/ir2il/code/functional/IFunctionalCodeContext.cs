namespace ZSharp.Runtime.NET.IR2IL.Code
{
    public interface IFunctionalCodeContext
        : ICodeContext
    {
        public IR.Function Function { get; }
    }
}
