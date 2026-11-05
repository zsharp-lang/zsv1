using ZSharp.AST;

namespace ZSharp.Parser
{
    public sealed class LoopBody
        : ParserContent<Statement, Statement>
    {
        public static LoopBody Content { get; } = new();

        private LoopBody() { }
    }
}
