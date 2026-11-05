using ZSharp.AST;

namespace ZSharp.Parser
{
    public sealed class MethodBody
        : ParserContent<Function, Statement>
    {
        public static MethodBody Content { get; } = new();

        private MethodBody() { }
    }
}
