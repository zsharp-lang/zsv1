namespace ZSharp.Importer.Literals
{
    partial class LiteralLoader
    {
        public required CompilerObject StringType { private get; init; }

        public CompilerObject Create(string value)
            => new Literal<string>()
            {
                Value = value,
                Type = new(StringType) { Value = value }
            };
    }
}
