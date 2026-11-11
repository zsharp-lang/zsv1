using ZSharp.AST;

namespace ZSharp.Parser
{
    public abstract class ParserContent<TNode, TContent>
        where TNode : Node
        where TContent : Node
    {
    }
}
