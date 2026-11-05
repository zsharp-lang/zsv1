using ZSharp.AST;

namespace ZSharp.Parser
{
    public delegate T ParserFunction<T>(Parser parser) where T : Node;

    public sealed class FunctionalParser<T>(ParserFunction<T> function)
        : Parser<T>
        where T : Node
    {
        private readonly ParserFunction<T> function = function;

        public override T Parse(Parser parser)
            => function(parser);
    }
}
