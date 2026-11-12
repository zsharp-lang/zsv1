using ZSharp.Parser; using ZSharp.Syntax; using ZSharp.Text; using ZSharp.Tokenizer;
var filePath = args[0];
ZSharp.AST.Document documentNode;
using (StreamReader stream = File.OpenText(filePath))
{
    var zsharpParser = new ZSharpParser();
    var parser = new Parser(Tokenizer.Tokenize(new(stream)));

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

    expressionParser.InfixR("=", 10);
    expressionParser.InfixL("<", 20);
    expressionParser.InfixL("+", 50);
    expressionParser.InfixL("-", 50);
    expressionParser.InfixL("*", 70);
    expressionParser.InfixL("/", 70);
    expressionParser.InfixL("**", 80);

    expressionParser.InfixL("==", 30);
    expressionParser.InfixL("!=", 30);

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

    //zsharpParser.Function.AddKeywordParser(
    //    LangParser.Keywords.While,
    //    Utils.ExpressionStatement(LangParser.ParseWhileExpression, semicolon: false)
    //);

    zsharpParser.RegisterParsers(parser);
    documentNode = zsharpParser.Parse(parser);

    Console.WriteLine($"Finished parsing document with {documentNode.Statements.Count} statements!");
}
foreach (var s in documentNode.Statements) Console.WriteLine(s.GetType().Name);
