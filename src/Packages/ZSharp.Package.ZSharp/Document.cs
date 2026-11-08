namespace Package.ZSharp
{
    /// <summary>
    /// A Z# document, as running it builds it. Importing a document gives this, and its
    /// members are what it exports.
    /// </summary>
    public sealed class Document(string path)
        : global::ZSharp.HIR.Instance
    {
        public string Path { get; set; } = path;

        public Dictionary<string, global::ZSharp.HIR.Instance> Exports { get; } = [];

        /// <summary>
        /// The locals of the document, in the order they were defined.
        /// </summary>
        public List<DocumentLocal> Locals { get; } = [];
    }
}
