namespace ZSharp.Importer.Literals
{
    internal sealed class Literal<T>
        : CompilerObject
        , ICompileIRCode
        , ITyped
    {
        private readonly CompilerObject type;

        public required T Value { get; init; }

        CompilerObject ITyped.Type => type;

        internal required LiteralType<T> Type {
            init => type = value;
        }
    }
}
