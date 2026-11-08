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
    }
}
