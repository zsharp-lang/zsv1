using ZSharp.AST;

namespace ZSharp.Parser
{
    public sealed class ConstructorBody
        : ParserContent<Constructor, Statement>
    {
        public static ConstructorBody Content { get; } = new();

        private ConstructorBody() { }
    }
}
