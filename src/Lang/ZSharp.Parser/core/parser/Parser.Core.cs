using ZSharp.Text;

namespace ZSharp.Parser
{
    public sealed partial class Parser(TokenStream tokens)
    {
        public TokenStream TokenStream { get; } = tokens;

        /// <summary>
        /// The path of the source being parsed, which each node it gives records, if it has
        /// one.
        /// </summary>
        public string? Source { get; set; }

        public Parser(IEnumerable<Token> tokens)
            : this(new TokenStream(tokens)) { }

        /// <summary>
        /// Where the next token starts, which is where what is parsed next starts.
        /// </summary>
        public Position Start => TokenStream.PeekToken().Span.Start;

        /// <summary>
        /// Records where a node that was parsed from <paramref name="start"/> up to the last
        /// token given was written, unless it already knows: a node parsed inside another
        /// parse, as an expression between parentheses, keeps the span of its own tokens.
        /// </summary>
        public T Located<T>(T node, Position start)
            where T : AST.Node
        {
            if (node is null || node.Location is not null)
                return node!;

            var end = TokenStream.Last is { } last ? Position.Max(start, last.Span.End) : start;

            node.Location = new(Source, new(start, end));

            return node;
        }
    }
}
