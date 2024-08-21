using ZSharp.Parser;
using ZSharp.Tokenizer;

using static MyParsers;


using (StreamReader stream = File.OpenText("./parserText.txt"))
{
    var parser = new Parser(Tokenizer.Tokenize(new(stream)));
    parser.AddParserFor(new ExpressionParser());

    var documentParser = new DocumentParser();

    documentParser.AddKeywordParser("let", ExpressionStatement(ParseLetExpression));
    
    using (var lookAhead = parser.LookAhead())
    {
        parser.Eat("let");

        using (var lookAhead2 = parser.LookAhead())
        {
            parser.Eat(ZSharp.Text.TokenType.Identifier);
        }

        lookAhead.Restore();
    }
    var documentNode = documentParser.Parse(parser);


    Console.WriteLine($"Finished parsing document with {documentNode.Statements.Count} statements!");
}
