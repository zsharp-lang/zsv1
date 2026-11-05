using ZSharp.AST;

namespace ZSharp.Parser
{
    public sealed class StackedParser<T>() : Parser<T>
        where T : Node
    {
        private readonly Parser<T>? defaultParser;
        private readonly Stack<Parser<T>> parsers = [];

        public StackedParser(Parser<T> parser, Parser<T>? defaultParser = null)
            : this()
        {
            this.defaultParser = defaultParser;
            parsers.Push(parser);
        }

        public void Push(Parser<T> parser)
            => parsers.Push(parser);

        public Parser<T> Pop()
            => parsers.Pop();

        public override T Parse(Parser parser)
        {
            foreach (var p in new Stack<Parser<T>>(parsers.Reverse()))
                using (var lookAhead = parser.LookAhead(commit: true))
                    try
                    {
                        return p.Parse(parser);
                    } catch (ParseError)
                    {
                        lookAhead.Restore();
                    }

            return defaultParser?.Parse(parser) ?? throw new ParseError();
        }
    }
}
