using ZSharp.CLI;
using ZSharp.Importer.ILLoader;
using ZSharp.Interpreter;
using ZSharp.Parser;
using ZSharp.SourceCompiler;
using ZSharp.SourceCompiler.Script;
using ZSharp.Text;
using ZSharp.Tokenizer;

var interpreter = new Interpreter();

var filePath = args.Length == 0 ? null : args[0];

if (filePath is null)
{
    interpreter.Log.Error("Missing input file path argument.", new CLIArgumentLogOrigin("<filePath:0>"));

    return;
}

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

#endregion

#region Setup Interpreter

new ZSharp.Compiler.CGDispatchers.Direct.Dispatcher(interpreter.Compiler).Apply();
new ZSharp.Compiler.CGDispatchers.Typed.Dispatcher(interpreter.Compiler).Apply();
new ZSharp.Compiler.CGDispatchers.Proxy.Dispatcher(interpreter.Compiler).Apply();

new ZSharp.Compiler.IRDispatchers.Static.Dispatcher(interpreter.Compiler).Apply();

new ZSharp.Compiler.EvaluatorDispatchers.Direct.Dispatcher(interpreter.Compiler).Apply();
new ZSharp.Compiler.EvaluatorDispatchers.IR.Dispatcher(interpreter.Compiler, interpreter.Runtime, interpreter.RTLoader).Apply();
var scriptCompiler = new ScriptCompiler(interpreter, documentNode, filePath);

interpreter.ILLoader.OnLoadOperator = (@operator, method) =>
{
    interpreter.Log.Warning(
        $"Skip loading operator {@operator} ({method.GetParameters().Length} parameters)" +
        $"because overloading is not implemented yet",
        ZSCScriptLogOrigin.Instance
    );
    //scriptCompiler.Context.Operators.Op(@operator.Operator, interpreter.ILLoader.LoadMethod(method));
};

StandardTypes.VoidType = interpreter.ILLoader.TypeSystem.Void;

#region Standard Operators

interpreter.Operators.Op(LangParser.Symbols.MemberAccess, interpreter.ILLoader.Expose(
    (Delegate)(
        (object obj, string memberName) =>
        {
            var type = obj.GetType();
            var property = type.GetProperty(memberName);
            if (property is not null)
                return property.GetValue(obj);
            var field = type.GetField(memberName);
            if (field is not null)
                return field.GetValue(obj);
            throw new Exception($"Member '{memberName}' not found on type '{type.FullName}'");
        }
    )
));

#endregion

#region Import System

var stringImporter = new StringImporter();

scriptCompiler.Context.ImportSystem.ImportFunction = interpreter.ILLoader.Expose(
    (Delegate)(
        (string source) => stringImporter.Import(source).Unwrap() as object
    )
);

#endregion

#region Importers

#region Core Library

var coreImporter = new CoreLibraryImporter();
stringImporter.RegisterImporter("core", coreImporter);

#endregion

#region Standard Library

var stdImporter = new StandardLibraryImporter();
stringImporter.RegisterImporter("std", stdImporter);

stdImporter.Add(
    "io",
    interpreter.ILLoader.LoadModule(typeof(Standard.IO.ModuleScope).Module)
);
stdImporter.Add(
    "fs",
    interpreter.ILLoader.LoadModule(typeof(Standard.FileSystem.ModuleScope).Module)
);
stdImporter.Add(
    "types",
    interpreter.ILLoader.LoadTypeAsModule(typeof(StandardTypes))
);
#endregion

#endregion

#endregion

#region Compilation

scriptCompiler.Compile();

#endregion


Console.WriteLine();

foreach (var log in interpreter.Log.Logs)
{
    Console.WriteLine(log.ToString());
}

Console.WriteLine();
Console.WriteLine("Press any key to exit...");
if (Console.ReadKey().Key == ConsoleKey.Z)
    Console.WriteLine("\bYou chose wisely :)");
