namespace ZSharp.Importer.Literals
{
    internal sealed class LiteralType<T>(CompilerObject type)
        : CompilerObject
    {
        public required T Value { get; init; }

        private CompilerObject PrimitiveType { get; } = type;
    }
}
