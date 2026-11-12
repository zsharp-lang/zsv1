using ZSharp.Parser;
using ZSharp.AST;

namespace ZSharp.Syntax
{
    using Parser = global::ZSharp.Parser.Parser;

    public sealed class LoopBody
        : ParserContent<Statement, Statement>
    {
        public static LoopBody Content { get; } = new();

        private LoopBody() { }
    }
}
