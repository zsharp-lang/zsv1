using ZSharp.AST;
using ZSharp.Parser;
using ZSharp.Text;
using ZSharp.Tokenizer;

using static MyParsers;


using (StreamReader stream = File.OpenText("./parserText.txt"))
{
    var parser = new Parser(Tokenizer.Tokenize(new(stream)));
    ExpressionParser expressionParser;
    parser.AddParserFor(expressionParser = new ExpressionParser());

    var documentParser = new DocumentParser();

    documentParser.AddKeywordParser("let", ParseLetStatement);

    expressionParser.Terminal(TokenType.String, token => new Literal(token.Value, LiteralType.String));



    var documentNode = documentParser.Parse(parser);

    Console.WriteLine($"Finished parsing document with {documentNode.Statements.Count} statements!");
}
