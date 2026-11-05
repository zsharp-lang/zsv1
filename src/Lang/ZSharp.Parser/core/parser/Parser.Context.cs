using CommonZ.Utils;
using ZSharp.AST;

namespace ZSharp.Parser
{
    public sealed partial class Parser
    {
        private readonly Mapping<Type, ParserBase> contextParsers = [];

        public void AddParserFor<TNode, TContent>(
            ParserContent<TNode, TContent> _,
            ContextParser<TNode, TContent> parser
        )
            where TNode : Node
            where TContent : Node
            => contextParsers[_.GetType()] = parser;

        public ContextParser<TNode, TContent>? GetParserFor<TNode, TContent>(
            ParserContent<TNode, TContent> _
        )
            where TNode : Node
            where TContent : Node
            => contextParsers[_.GetType()] as ContextParser<TNode, TContent>;

        public ContextManager Stack<TNode, TContent>(ParserContent<TNode, TContent> content)
            where TNode : Node
            where TContent : Node
            => Stack(new FunctionalParser<TContent>((GetParserFor(content) ?? throw new()).ParseContextItem));

        public ContextManager Stack<T>(Parser<T> parser)
            where T : Node
        {
            if (GetParserFor<T>() is not StackedParser<T> stacked)
                throw new();

            stacked.Push(parser);

            return new(() => stacked.Pop());
        }

        public ContextManager NewStack<TNode, TContent>(ParserContent<TNode, TContent> content, Parser<TContent>? defaultParser = null)
            where TNode : Node
            where TContent : Node
            => NewStack(new FunctionalParser<TContent>((GetParserFor(content) ?? throw new()).ParseContextItem), defaultParser);

        public ContextManager NewStack<T>(Parser<T> parser, Parser<T>? defaultParser)
            where T : Node
            => Override(new StackedParser<T>(parser, defaultParser));

        public ContextManager Override<T>(Parser<T> parser)
            where T : Node
        {
            (contextParsers[typeof(T)], parser) = (parser, (Parser<T>)contextParsers[typeof(T)]);

            return new ContextManager(() =>
            {
                contextParsers[typeof(T)] = parser;
            });
        }
    }
}
