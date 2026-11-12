using ZSharp.Parser;
using ZSharp.AST;

namespace ZSharp.Syntax
{
    using Parser = global::ZSharp.Parser.Parser;

    public sealed class ConstructorBody
        : ParserContent<Constructor, Statement>
    {
        public static ConstructorBody Content { get; } = new();

        private ConstructorBody() { }
    }
}
