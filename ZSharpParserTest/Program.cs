using ZSharp.AST;
using ZSharp.Parser;
using ZSharp.Tokenizer;

using static MyParsers;


using (StreamReader stream = File.OpenText("./parserText.txt"))
{
    var parser = new Parser(Tokenizer.Tokenize(new(stream)));
    parser.AddContextParser(new ExpressionParser());

    var documentParser = new DocumentParser();

    documentParser.AddKeywordParser("let", ExpressionStatement(ParseLetExpression));

    var documentNode = documentParser.Parse(parser);

    Console.WriteLine($"Finished parsing document with {documentNode.Statements.Count} statements!");
}
