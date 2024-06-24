namespace ZSharp.CTRuntime
{
    public sealed class Document(string path) : IR.IRObject
    {
        public string Path { get; } = path;


    }
}
