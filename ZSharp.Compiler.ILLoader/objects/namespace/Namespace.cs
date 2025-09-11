namespace ZSharp.Compiler.ILLoader.Objects
{
    public sealed partial class Namespace(string name, ILLoader loader)
        : CompilerObject
    {
        public string Name { get; set; } = name;

        public bool IsAnonymous => Name == string.Empty;
    }
}
