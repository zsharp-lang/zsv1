using ZSharp.Text;

namespace ZSharp.Parser
{
    public static class ExpressionParserExtensions
    {
        public static void InfixL(this ExpressionParser expressionParser, string @operator, int bindingPower)
            => expressionParser.Led(@operator, Utils.InfixL(@operator, bindingPower), bindingPower);

        public static void InfixR(this ExpressionParser expressionParser, string @operator, int bindingPower)
            => expressionParser.Led(@operator, Utils.InfixR(@operator, bindingPower), bindingPower);

        public static void Separator(this ExpressionParser expressionParser, string symbol)
            => expressionParser.Nud(symbol, parser => null!, -1);

        public static void Separator(this ExpressionParser expressionParser, TokenType type)
            => expressionParser.Nud(type, parser => null!, -1);

        public static void Terminal(this ExpressionParser expressionParser, string value, TerminalFunction function)
            => expressionParser.Nud(value, parser => function(parser.Eat(value)), 1);

        public static void Terminal(this ExpressionParser expressionParser, TokenType type, TerminalFunction function)
            => expressionParser.Nud(type, parser => function(parser.Eat(type)), 1);
    }
}
