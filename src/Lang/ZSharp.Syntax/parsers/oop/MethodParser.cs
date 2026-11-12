using ZSharp.Parser;
using ZSharp.AST;
using ZSharp.Text;

namespace ZSharp.Syntax
{
    using Parser = global::ZSharp.Parser.Parser;

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

            return ParseRest<Function>(parser, funKeyword, name, (tokens, parts) => new(tokens)
            {
                Body = parts.Body,
                GenericParameters = parts.GenericParameters,
                Name = parts.Name,
                ReturnType = parts.ReturnType,
                Signature = parts.Signature,
            });
        }

        /// <summary>
        /// An accessor of a property, <c>get Name(): T { ... }</c> or
        /// <c>set Name(value: T) { ... }</c>: a function named by its property.
        /// </summary>
        public Accessor ParseAccessor(Parser parser)
        {
            var isSetter = parser.Is(LangParser.Keywords.Set);
            var keyword = parser.Eat(isSetter ? LangParser.Keywords.Set : LangParser.Keywords.Get);
            var name = parser.Eat(TokenType.Identifier).Value;

            return ParseRest<Accessor>(parser, keyword, name, (tokens, parts) => new(tokens)
            {
                Body = parts.Body,
                GenericParameters = parts.GenericParameters,
                Name = parts.Name,
                ReturnType = parts.ReturnType,
                Signature = parts.Signature,
                Kind = isSetter ? AccessorKind.Set : AccessorKind.Get,
            });
        }

        private sealed record Parts(string? Name, List<GenericParameter>? GenericParameters, Signature Signature, Expression? ReturnType, Statement? Body);

        private T ParseRest<T>(Parser parser, Token keyword, string? name, Func<FunctionTokens, Parts, T> make)
            where T : Function
        {
            var funKeyword = keyword;

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

            return make(new() { FunKeyword = funKeyword }, new(name, genericParameters, signature, returnType, body));
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
