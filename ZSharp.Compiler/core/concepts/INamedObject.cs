namespace ZSharp.Compiler
{
    public interface INamedObject
        : CompilerObject
    {
        public string Name { get; }

        public bool IsAnonymous => Name == string.Empty;
    }
}
