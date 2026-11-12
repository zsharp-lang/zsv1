using ZSharp.AST;

namespace ZSharp.Parser
{
    public abstract class Parser<TNode> : ParserBase
        where TNode : Node
    {
        public abstract TNode Parse(Parser parser);
    }
}
