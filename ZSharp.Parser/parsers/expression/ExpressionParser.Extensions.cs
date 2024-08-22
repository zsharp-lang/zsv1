using CommonZ.Utils;
using ZSharp.AST;
using ZSharp.Text;

namespace ZSharp.Parser
{
    public delegate Expression NudFunction(Parser parser);

    public delegate Expression LedFunction(Parser parser, Expression left);


    public sealed partial class ExpressionParser
    {
        private class SubExpressionParser
        {
            public int BindingPower { get; set; }

            public NudFunction? Nud { get; set; }

            public LedFunction? Led { get; set; }
        }

        private readonly Mapping<string, SubExpressionParser> subParserByValue = [];
        private readonly Mapping<TokenType, SubExpressionParser> subParserByType = [];

        public void Nud(string value, NudFunction nudFunction, int bindingPower = 0)
        {
            if (!subParserByValue.TryGetValue(value, out var subParser))
                subParserByValue[value] = subParser = new()
                {
                    BindingPower = bindingPower
                };

            subParser.Nud = nudFunction;
        }

        public void Led(string value, LedFunction ledFunction, int bindingPower = 0)
        {
            if (!subParserByValue.TryGetValue(value, out var subParser))
                subParserByValue[value] = subParser = new()
                {
                    BindingPower = bindingPower
                };

            subParser.Led = ledFunction;
        }

        public void Nud(TokenType type, NudFunction nudFunction, int bindingPower = 0)
        {
            if (!subParserByType.TryGetValue(type, out var subParser))
                subParserByType[type] = subParser = new()
                {
                    BindingPower = bindingPower
                };

            subParser.Nud = nudFunction;
        }

        public void Led(TokenType type, LedFunction ledFunction, int bindingPower = 0)
        {
            if (!subParserByType.TryGetValue(type, out var subParser))
                subParserByType[type] = subParser = new()
                {
                    BindingPower = bindingPower
                };

            subParser.Led = ledFunction;
        }

        private SubExpressionParser? GetSubParserFor(TokenType type)
            => subParserByType.TryGetValue(type, out var parser) ? parser : null;

        private SubExpressionParser? GetSubParserFor(string value)
            => subParserByValue.TryGetValue(value, out var parser) ? parser : null;

        private SubExpressionParser? GetSubParserFor(Token token)
            => GetSubParserFor(token.Value) ?? GetSubParserFor(token.Type);
    }
}
