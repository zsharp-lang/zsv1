namespace Package.ZSharp
{
    /// <summary>
    /// A local of a document. It lives as long as the document does, and starts where a
    /// <c>Declare</c> of it is, with its initial value if it has one. It isn't a variable:
    /// the document runs as it is compiled, so it has a value by the time anything after
    /// it is compiled.
    /// </summary>
    public sealed class DocumentLocal(Document document, string name, global::ZSharp.HIR.Type type)
        : global::ZSharp.HIR.Definition
        , global::ZSharp.HIR.Instance
    {
        public Document Document { get; set; } = document;

        public string Name { get; set; } = name;

        public global::ZSharp.HIR.Type Type { get; set; } = type;

        public global::ZSharp.HIR.Code.Expression? Init { get; set; }
    }
}
