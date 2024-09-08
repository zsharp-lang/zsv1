using ZSharp.AST;

namespace ZSharp.Parser
{
    public sealed class ClassBody
        : ParserContent<OOPDefinition, Statement>
    {
        public static ClassBody Content { get; } = new();

        private ClassBody() { }
    }
}
