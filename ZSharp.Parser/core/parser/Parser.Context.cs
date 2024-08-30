using CommonZ.Utils;
using ZSharp.AST;

namespace ZSharp.Parser
{
    public sealed partial class Parser
    {
        private readonly Mapping<Type, ParserBase> contextParsers = [];

        public void AddParserFor<TNode, TContent>(
            ParserContent<TNode, TContent>_,
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
    }
}
