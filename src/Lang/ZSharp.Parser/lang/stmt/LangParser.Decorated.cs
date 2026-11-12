using ZSharp.AST;

namespace ZSharp.Parser
{
    public static partial class LangParser
    {
        /// <summary>
        /// Parses <c>@decorator</c> and then whatever the context parses next. The
        /// decorator is an expression made of names, member accesses and calls.
        /// </summary>
        public static ParserFunction<Statement> ParseDecoratedStatement<TNode>(ContextParser<TNode, Statement> context)
            where TNode : Node
            => parser =>
            {
                parser.Eat(Symbols.Decorator);

                var decorator = (parser.GetParserFor<Expression>() as ExpressionParser ?? throw new ParseError())
                    .ParseExpression(parser, DecoratorBindingPower);

                return new DecoratedStatement
                {
                    Decorator = decorator,
                    Statement = context.ParseContextItem(parser)
                };
            };

        /// <summary>
        /// Calls and member accesses bind tighter than this, and nothing else does.
        /// </summary>
        private const int DecoratorBindingPower = 99;
    }
}
