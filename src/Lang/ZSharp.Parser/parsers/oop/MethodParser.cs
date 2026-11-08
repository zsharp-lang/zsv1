using ZSharp.AST;
using ZSharp.Text;

namespace ZSharp.Parser
{
    public sealed class MethodParser
        : ContextParser<Function, Statement>
    {
        public MethodParser()
        {
            AddKeywordParser(
                LangParser.Keywords.Return,
                LangParser.ParseReturnStatement
            );
            AddKeywordParser(
                LangParser.Keywords.Let,
                Utils.ExpressionStatement(LangParser.ParseLetExpression)
            );
        }

        public override Function Parse(Parser parser)
        {
            var funKeyword = parser.Eat(LangParser.Keywords.Function);

            string? name = null;
            if (parser.Is(TokenType.Identifier))
                name = parser.Eat(TokenType.Identifier).Value;

            List<GenericParameter>? genericParameters = null;
            if (parser.Is(TokenType.LBracket, eat: true))
            {
                genericParameters = [];

                do
                {
                    genericParameters.Add(LangParser.ParseGenericParameter(parser));
                } while (parser.Is(TokenType.Comma, eat: true));

                parser.Eat(TokenType.RBracket);
            }

            parser.Eat(TokenType.LParen);

            var signature = LangParser.ParseSignature(parser);

            parser.Eat(TokenType.RParen);

            // TODO: add support for modifiers
            //var modifiers = ParseModifiers(parser); 

            Expression? returnType = null;
            if (parser.Is(TokenType.Colon, eat: true))
                returnType = parser.Parse<Expression>();

            // A method with no body, as an abstract or an interface's, ends with ;.
            Statement? body = parser.Is(TokenType.Semicolon, eat: true) ? null : ParseFunctionBody(parser);

            return new(new()
            {
                FunKeyword = funKeyword,
            })
            {
                Body = body,
                GenericParameters = genericParameters,
                Name = name,
                ReturnType = returnType,
                Signature = signature,
            };
        }

        protected override Statement ParseDefaultContextItem(Parser parser)
            => LangParser.ParseExpressionStatement(parser);

        private Statement ParseFunctionBody(Parser parser)
        {
            using (parser.NewStack(MethodBody.Content, parser.GetParserFor<Statement>()))
            {
                if (parser.Is(LangParser.Symbols.ThenDo, eat: true))
                    return ParseContextItem(parser);

                return LangParser.ParseBlockStatement(parser);
            }
        }
    }
}
