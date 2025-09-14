namespace ZSharp.Platform.Runtime.Loaders
{
    public interface IFunctionalCodeContext
        : ICodeContext
    {
        public IR.Function Function { get; }
    }
}
