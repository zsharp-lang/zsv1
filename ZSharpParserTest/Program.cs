using ZSharp.AST;
using ZSharp.Parser;
using ZSharp.Tokenizer;


using (StreamReader stream = File.OpenText("./parserText.txt"))
{
    var parser = new Parser(Tokenizer.Tokenize(new(stream)));
    parser.AddContextParser(new ExpressionParser());

    var documentParser = new DocumentParser();

    documentParser.AddKeywordParser(
        "let", parser =>
        {
            var expression = MyParsers.ParseLetExpression(parser);
            parser.Eat(ZSharp.Text.TokenType.Semicolon);
            return new ExpressionStatement(expression);
        }
    );

    var documentNode = documentParser.Parse(parser);

    Console.WriteLine($"Finished parsing document with {documentNode.Statements.Count} statements!");
}
