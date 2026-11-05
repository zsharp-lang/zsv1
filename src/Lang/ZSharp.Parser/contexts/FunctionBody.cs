using ZSharp.AST;

namespace ZSharp.Parser
{
    public sealed class FunctionBody
        : ParserContent<Function, Statement>
    {
        public static FunctionBody Content { get; } = new();

        private FunctionBody() { }
    }
}
