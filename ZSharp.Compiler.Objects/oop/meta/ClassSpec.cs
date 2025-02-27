namespace ZSharp.Objects
{
    public sealed class ClassSpec
        : CompilerObject
    {
        public string Name { get; set; } = string.Empty;

        public CompilerObject[] Bases { get; set; } = [];

        public CompilerObject[] Content { get; set; } = [];
    }
}
