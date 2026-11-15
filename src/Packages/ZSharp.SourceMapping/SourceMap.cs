namespace ZSharp.SourceMapping
{
    /// <summary>
    /// Where something came from: a span of a document, by line and column, each counted
    /// from 1, the end just after the span's last character. It is data only, constants a
    /// platform can write, so nothing about it depends on the language it was written in.
    /// </summary>
    public sealed class SourceMap(string document, int startLine, int startColumn, int endLine, int endColumn)
    {
        /// <summary>
        /// The document's path.
        /// </summary>
        public string Document { get; } = document;

        public int StartLine { get; } = startLine;

        public int StartColumn { get; } = startColumn;

        public int EndLine { get; } = endLine;

        public int EndColumn { get; } = endColumn;

        public override string ToString() => $"{Document}({StartLine},{StartColumn})";
    }
}
