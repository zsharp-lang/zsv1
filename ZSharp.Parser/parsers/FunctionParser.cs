using ZSharp.AST;
using ZSharp.Text;

namespace ZSharp.Parser
{
    public sealed class FunctionParser
        : ContextParser<Function, Statement>
    {
        public FunctionParser()
        {
            AddKeywordParser(
                LangParser.Keywords.Return,
                LangParser.ParseReturnStatement
            );
            AddKeywordParser(
                LangParser.Keywords.Let,
                Utils.ExpressionStatement(LangParser.ParseLetExpression)
            );
            AddKeywordParser(
                LangParser.Keywords.Var,
                Utils.ExpressionStatement(LangParser.ParseVarExpression)
            );
        }

        public override Function Parse(Parser parser)
        {
            var funKeyword = parser.Eat(LangParser.Keywords.Function);

            string name = string.Empty;
            if (parser.Is(TokenType.Identifier))
                name = parser.Eat(TokenType.Identifier).Value;

            parser.Eat(TokenType.LParen);

            var signature = LangParser.ParseSignature(parser);

            parser.Eat(TokenType.RParen);

            // TODO: add support for modifiers
            //var modifiers = ParseModifiers(parser); 

            Expression? returnType = null;
            if (parser.Is(TokenType.Colon, eat: true))
                returnType = parser.Parse<Expression>();

            var body = ParseFunctionBody(parser);

            return new(new()
            {
                FunKeyword = funKeyword,
            })
            {
                Body = body,
                Name = name,
                ReturnType = returnType,
                Signature = signature,
            };
        }

        protected override Statement ParseDefaultContextItem(Parser parser)
            => throw new ParseError();

        private Statement ParseFunctionBody(Parser parser)
        {
            using (parser.NewStack(FunctionBody.Content, parser.GetParserFor<Statement>()))
            {
                if (parser.Is(LangParser.Symbols.ThenDo, eat: true))
                    return ParseContextItem(parser);

                return LangParser.ParseBlockStatement(parser);
            }
        }
    }
}
