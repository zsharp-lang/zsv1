using ZSharp.Parser;
using ZSharp.AST;

namespace ZSharp.Syntax
{
    using Parser = global::ZSharp.Parser.Parser;

    public sealed class FunctionBody
        : ParserContent<Function, Statement>
    {
        public static FunctionBody Content { get; } = new();

        private FunctionBody() { }
    }
}
