using System.Text;
using ZSharp.Compiler;
using ZSharp.Interpreter;
using ZSharp.Parser;
using ZSharp.SourceCompiler.Project;
using ZSharp.Text;
using ZSharp.Tokenizer;


#if false
#region Parsing

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
            _ => new ZSharp.AST.IdentifierExpression(token.Value),
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
        LangParser.Keywords.Class,
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
    expressionParser.Led(LangParser.Keywords.Is, LangParser.ParseIsOfExpression, 20);

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

    //zsharpParser.Function.AddKeywordParser(
    //    LangParser.Keywords.While,
    //    Utils.ExpressionStatement(LangParser.ParseWhileExpression, semicolon: false)
    //);

    zsharpParser.RegisterParsers(parser);
    documentNode = zsharpParser.Parse(parser);

    Console.WriteLine($"Finished parsing document with {documentNode.Statements.Count} statements!");
}

#endregion
#endif

#region Compilation

var interpreter = new Interpreter();


new Referencing(interpreter.Compiler);
new OOP(interpreter.Compiler);

var projectRoot = args.Length == 0 
    ? Standard.FileSystem.Directory.CurrentWorkingDirectory() 
    : Standard.FileSystem.Directory.From(args[0]) ?? throw new ArgumentException($"Path {args[0]} does not point to a valid directory")
    ;
var project = new Project(projectRoot.Name, projectRoot, projectRoot / "src" as Standard.FileSystem.Directory);
ProjectDiscovery.Discover(project);

#endregion

Console.WriteLine();

Console.WriteLine();
Console.WriteLine("Press any key to exit...");
Console.ReadKey();
