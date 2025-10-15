namespace ZSharp.Platform.Runtime.Loaders
{
    public interface ICodeContext : IContext
    {
        public Emit.ILGenerator IL { get; }

        public CodeStack Stack { get; }

        public Runtime Runtime { get; }
    }
}
