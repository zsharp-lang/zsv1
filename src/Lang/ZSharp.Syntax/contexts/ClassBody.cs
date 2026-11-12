using ZSharp.Parser;
using ZSharp.AST;

namespace ZSharp.Syntax
{
    using Parser = global::ZSharp.Parser.Parser;

    public sealed class ClassBody
        : ParserContent<TypeDefinition, Statement>
    {
        public static ClassBody Content { get; } = new();

        private ClassBody() { }
    }
}
