namespace ZSharp.AST
{
    /// <summary>
    /// Where a node was written: the path of the source it was parsed from, if it has one,
    /// and its span there. Nodes of several sources can be put together, as a module's
    /// are, so each keeps its own.
    /// </summary>
    public sealed class Location(string? path, Text.Span span)
    {
        public string? Path { get; } = path;

        public Text.Span Span { get; } = span;
    }
}
