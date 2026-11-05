using ZSharp.AST;

namespace ZSharp.Parser
{
    public sealed class ClassBody
        : ParserContent<TypeDefinition, Statement>
    {
        public static ClassBody Content { get; } = new();

        private ClassBody() { }
    }
}
