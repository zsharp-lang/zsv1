using ZSharp.Parser;
using ZSharp.AST;
using ZSharp.Text;

namespace ZSharp.Syntax
{
    using Parser = global::ZSharp.Parser.Parser;

    public static partial class LangParser
    {
        public static BlockStatement ParseBlockStatement(Parser parser)
        {
            parser.Eat(TokenType.LCurly);

            List<Statement> statements = [];

            while (!parser.Is(TokenType.RCurly))
                statements.Add(parser.Parse<Statement>());

            parser.Eat(TokenType.RCurly);

            return new()
            {
                Statements = statements,
            };
        }
	}
}
