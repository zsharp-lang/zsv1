using ZSharp.Parser;
using ZSharp.Text;

namespace ZSharp.Lang
{
    /// <summary>
    /// The grammar of the built-in language. It is registered, not hardcoded, so an
    /// extension can change it between statements.
    /// </summary>
    public static class Syntax
    {
        public static ZSharpParser Install(ZSharp.Parser.Parser target)
        {
            var zsharpParser = new ZSharpParser();

            var expressionParser = zsharpParser.Expression;
            var statementParser = zsharpParser.Statement;

            expressionParser.Terminal(
                TokenType.String,
                token => new ZSharp.AST.LiteralExpression(token.Value, ZSharp.AST.LiteralType.String)
            );
            expressionParser.Terminal(
                TokenType.Number,
                token => new ZSharp.AST.LiteralExpression(token.Value, ZSharp.AST.LiteralType.Number)
            );
            expressionParser.Terminal(
                TokenType.Decimal,
                token => new ZSharp.AST.LiteralExpression(token.Value, ZSharp.AST.LiteralType.Decimal)
            );
            expressionParser.Terminal(
                TokenType.Identifier,
                token => token.Value switch
                {
                    "null" => ZSharp.AST.LiteralExpression.Null(),
                    "true" => ZSharp.AST.LiteralExpression.True(),
                    "false" => ZSharp.AST.LiteralExpression.False(),
                    _ => new ZSharp.AST.IdentifierExpression(new(token)),
                }
            );
            expressionParser.Nud(
                TokenType.LParen,
                parser =>
                {
                    parser.Eat(TokenType.LParen);
                    var expression = parser.Parse<ZSharp.AST.Expression>();
                    parser.Eat(TokenType.RParen);

                    return expression;
                },
                10000
            );
            expressionParser.Nud(
                LangParser.Keywords.Let,
                LangParser.ParseLetExpression
            );
            expressionParser.Nud(
                LangParser.Keywords.Def,
                zsharpParser.Class.Parse
            );
            expressionParser.Nud(
                LangParser.Keywords.Module,
                zsharpParser.Module.Parse
            );

            expressionParser.InfixR("=", 10);
            expressionParser.InfixL("<", 20);
            expressionParser.InfixL(">", 20);
            expressionParser.InfixL("<=", 20);
            expressionParser.InfixL(">=", 20);
            expressionParser.InfixL("+", 50);
            expressionParser.InfixL("-", 50);
            expressionParser.InfixL("*", 70);
            expressionParser.InfixL("/", 70);
            expressionParser.InfixL("**", 80);

            // A minus before an operand negates it, binding tighter than `*`, as in C#, and
            // looser than `**`, as `-2 ** 2` is `-(2 ** 2)`.
            expressionParser.Prefix("-", 75);

            // Equality binds looser than comparisons and type tests, as in C#, so
            // `a == b as T` is `a == (b as T)` and `a < b == c` is `(a < b) == c`.
            expressionParser.InfixL("==", 18);
            expressionParser.InfixL("!=", 18);

            // Bitwise operators bind looser than equality, `&` tighter than `|`, as in C#.
            expressionParser.InfixL("&", 17);
            expressionParser.InfixL("|", 16);

            expressionParser.InfixL(LangParser.Keywords.Or, 15);

            expressionParser.Led(TokenType.LParen, LangParser.ParseCallExpression, 100);
            expressionParser.Led(TokenType.LBracket, LangParser.ParseIndexExpression, 100);
            expressionParser.Nud(TokenType.LBracket, LangParser.ParseArrayLiteral);
            expressionParser.Led(".", LangParser.ParseMemberAccess, 150);
            expressionParser.Led(LangParser.Keywords.As, LangParser.ParseCastExpression, 20);
            expressionParser.Led(LangParser.Keywords.Is, LangParser.ParseIsExpression, 20);

            expressionParser.Separator(TokenType.Comma);
            expressionParser.Separator(TokenType.RParen);
            expressionParser.Separator(TokenType.RBracket);
            expressionParser.Separator(TokenType.Semicolon);

            expressionParser.Separator(LangParser.Keywords.In); // until it's an operator
            expressionParser.Separator(LangParser.Keywords.From); // as in break value from loop

            expressionParser.AddKeywordParser(
                LangParser.Keywords.While,
                LangParser.ParseWhileExpression<ZSharp.AST.Expression>
            );

            statementParser.AddKeywordParser(
                LangParser.Keywords.While,
                Utils.ExpressionStatement(LangParser.ParseWhileExpression<ZSharp.AST.Statement>, semicolon: false)
            );

            statementParser.AddKeywordParser(
                LangParser.Keywords.If,
                LangParser.ParseIfStatement
            );

            statementParser.AddKeywordParser(
                LangParser.Keywords.For,
                LangParser.ParseForStatement
            );

            statementParser.AddKeywordParser(
                LangParser.Keywords.Case,
                LangParser.ParseCaseStatement
            );

            zsharpParser.Document.AddKeywordParser(
                LangParser.Keywords.If,
                LangParser.ParseIfStatement
            );

            zsharpParser.Document.AddKeywordParser(
                LangParser.Keywords.While,
                Utils.ExpressionStatement(LangParser.ParseWhileExpression<ZSharp.AST.Statement>, semicolon: false)
            );

            // A function can be defined in code. What it is depends on the code it is in.
            statementParser.AddKeywordParser(
                LangParser.Keywords.Function,
                Utils.DefinitionStatement(zsharpParser.Function.Parse)
            );

            //zsharpParser.Function.AddKeywordParser(
            //    LangParser.Keywords.While,
            //    Utils.ExpressionStatement(LangParser.ParseWhileExpression, semicolon: false)
            //);


            Decorated(zsharpParser.Document);
            Decorated(zsharpParser.Module);
            Decorated(zsharpParser.Class);
            Decorated(zsharpParser.Class.Method);
            Decorated(zsharpParser.Class.Constructor);
            Decorated(zsharpParser.Function);
            Decorated(zsharpParser.Statement);

            // An import runs where it is, so it can be anywhere a statement can.
            Imports(zsharpParser.Class.Method);
            Imports(zsharpParser.Class.Constructor);
            Imports(zsharpParser.Function);
            Imports(zsharpParser.Statement);

            // A loop at the start of a statement is a loop statement, not an expression.
            Loops(zsharpParser.Class.Method);
            Loops(zsharpParser.Class.Constructor);
            Loops(zsharpParser.Function);

            zsharpParser.RegisterParsers(target);

            return zsharpParser;
        }

        private static void Imports<T>(ContextParser<T, ZSharp.AST.Statement> parser)
            where T : ZSharp.AST.Node
            => parser.AddKeywordParser(LangParser.Keywords.Import, LangParser.ParseImportStatement);

        private static void Loops<T>(ContextParser<T, ZSharp.AST.Statement> parser)
            where T : ZSharp.AST.Node
            => parser.AddKeywordParser(
                LangParser.Keywords.While,
                Utils.ExpressionStatement(LangParser.ParseWhileExpression<ZSharp.AST.Statement>, semicolon: false)
            );

        private static void Decorated<T>(ContextParser<T, ZSharp.AST.Statement> parser)
            where T : ZSharp.AST.Node
            => parser.AddKeywordParser(LangParser.Symbols.Decorator, LangParser.ParseDecoratedStatement(parser));
    }
}
