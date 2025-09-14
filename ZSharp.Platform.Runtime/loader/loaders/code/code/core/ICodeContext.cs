namespace ZSharp.Platform.Runtime.Loaders
{
    public interface ICodeContext
    {
        public Emit.ILGenerator IL { get; }

        public CodeStack Stack { get; }

        public Runtime Runtime { get; }
    }
}
