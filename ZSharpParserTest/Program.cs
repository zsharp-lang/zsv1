using ZSharp.AST;
using ZSharp.Parser;
using ZSharp.Text;
using ZSharp.Tokenizer;


using (StreamReader stream = File.OpenText("./parserText.txt"))
{
    var zsharpParser = new ZSharpParser();
    var parser = new Parser(Tokenizer.Tokenize(new(stream)));

    var expressionParser = zsharpParser.Expression;

    expressionParser.Terminal(TokenType.String, token => new LiteralExpression(token.Value, LiteralType.String));

    expressionParser.InfixL("+", 50);
    expressionParser.InfixL("*", 70);
    expressionParser.InfixL("**", 80);

    expressionParser.Led(TokenType.LParen, LangParser.ParseCallExpression, 100);

    expressionParser.AddKeywordParser(LangParser.Keywords.Function, LangParser.ParseFunctionExpression);

    expressionParser.Separator(TokenType.Comma);
    expressionParser.Separator(TokenType.RParen);
    expressionParser.Separator(TokenType.Semicolon);
    
    zsharpParser.RegisterParsers(parser);
    var documentNode = zsharpParser.Parse(parser);

    Console.WriteLine($"Finished parsing document with {documentNode.Statements.Count} statements!");

    Console.WriteLine();

    foreach (var statement in documentNode.Statements)
    {
        Console.WriteLine();
        Console.WriteLine(statement.GetType().Name);
        Console.WriteLine(statement);
    }
}
