namespace ZSharp.Objects
{
    public sealed class ClassSpecBuilder
        : CompilerObject
    {
        public string Name { get; set; } = string.Empty;

        public List<CompilerObject> Bases { get; } = [];

        public List<CompilerObject> Content { get; } = [];

        public ClassSpec CreateSpec()
            => new()
            {
                Name = Name,
                Bases = [.. Bases],
                Content = [.. Content],
            };
    }
}
