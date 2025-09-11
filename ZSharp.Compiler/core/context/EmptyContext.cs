namespace ZSharp.Compiler
{
    internal sealed class EmptyContext
        : IContext
    {
        IContext? IContext.Parent
        {
            get => null;
            set => throw new InvalidOperationException("Empty context cannot be mounted.");
        }
    }
}
