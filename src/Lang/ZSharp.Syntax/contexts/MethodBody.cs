using ZSharp.Parser;
using ZSharp.AST;

namespace ZSharp.Syntax
{
    using Parser = global::ZSharp.Parser.Parser;

    public sealed class MethodBody
        : ParserContent<Function, Statement>
    {
        public static MethodBody Content { get; } = new();

        private MethodBody() { }
    }
}
