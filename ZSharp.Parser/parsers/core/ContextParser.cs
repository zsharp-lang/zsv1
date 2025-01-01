using CommonZ.Utils;
using ZSharp.AST;

namespace ZSharp.Parser
{
    public abstract class ContextParser<TNode, TContent>
        : Parser<TNode>
        where TNode : Node
        where TContent : Node
    {
        private readonly Mapping<string, Parser<TContent>> keywordParsers = [];

        public void AddKeywordParser(string keyword, Parser<TContent> parser)
            => keywordParsers.Add(keyword, parser);

        public void AddKeywordParser(string keyword, ParserFunction<TContent> parser)
            => AddKeywordParser(keyword, new FunctionalParser<TContent>(parser));

        internal protected TContent ParseContextItem(Parser parser)
        {
            if (parser.Is(Text.TokenType.Identifier, out var identifier))
                if (keywordParsers.TryGetValue(identifier.Value, out var keywordParser))
                    return keywordParser.Parse(parser);
            return ParseDefaultContextItem(parser);
        }

        protected abstract TContent ParseDefaultContextItem(Parser parser);
    }
}
