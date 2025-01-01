using CommonZ.Utils;

namespace ZSharp.ZSSourceCompiler
{
    public sealed class Document(string path) 
        : CompilerObject
    {
        public string Path { get; } = path;

        public Collection<CompilerObject> Content { get; } = [];

        public Mapping<string, CompilerObject> Members { get; } = [];
    }
}
