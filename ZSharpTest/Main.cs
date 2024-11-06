using ZSharp.Compiler;
using ZSharp.Parser;
using ZSharp.Text;
using ZSharp.Tokenizer;

string FileName = args.Length == 0 ? "test.zs" : args[0];


#region Parsing

ZSharp.AST.Document documentNode;
using (StreamReader stream = File.OpenText(FileName))
{
    var zsharpParser = new ZSharpParser();
    var parser = new Parser(Tokenizer.Tokenize(new(stream)));
    
    var expressionParser = zsharpParser.Expression;

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
        LangParser.Keywords.Let,
        LangParser.ParseLetExpression
    );

    expressionParser.InfixL("+", 50);
    expressionParser.InfixL("*", 70);
    expressionParser.InfixL("**", 80);

    expressionParser.Led(TokenType.LParen, LangParser.ParseCallExpression, 100);
    expressionParser.Led(".", LangParser.ParseMemberAccess, 150);

    expressionParser.Separator(TokenType.Comma);
    expressionParser.Separator(TokenType.RParen);
    expressionParser.Separator(TokenType.Semicolon);

    zsharpParser.RegisterParsers(parser);
    documentNode = zsharpParser.Parse(parser);

    Console.WriteLine($"Finished parsing document with {documentNode.Statements.Count} statements!");
}

#endregion


#region Resolving


var rastNodes = ZSharp.Resolver.Resolver.Resolve(documentNode).ToArray();


#endregion


#region Compilation

var compiler = new Compiler(ZSharp.IR.RuntimeModule.Standard);

var standardModule = new ZSharp.CT.StandardLibrary.StandardModule();
compiler.ImportIR(compiler.Runtime.AddInternalModule(standardModule));

compiler.Operators.Cache("+", standardModule.AdditionOperator);
var overloads = new List<ZSharp.CGObjects.RTFunction>(standardModule.AdditionOperator.Overloads);
standardModule.AdditionOperator.Overloads.Clear();
standardModule.AdditionOperator.Overloads.AddRange(overloads.Select(o => o.IR).Select(compiler.ImportIR).Select(o =>
{
    ZSharp.CT.StandardLibrary.InternalFunction r = new(o.IR!)
    {
        Implementation = standardModule.FunctionImplementations[o.IR!]
    };

    (o.Signature, r.Signature) = (r.Signature, o.Signature);
    return r;
}));

foreach (var overload in standardModule.AdditionOperator.Overloads)
    compiler.Runtime.GetInternalFunction(overload.IR!);

compiler.Context.GlobalScope.Cache("import", standardModule.Import);

var module = compiler.CompileAsDocument(rastNodes);

Console.WriteLine("Compilation finished!");

#endregion

Console.WriteLine();

ZSharp.IR.Module? mainModule = null;
foreach (var submodule in module.Submodules)
{
    if (submodule.Name == "Program")
        if (mainModule is not null) throw new Exception("Multiple main modules found!");
        else mainModule = submodule;
}

if (mainModule is null) Console.WriteLine("No main module found!");
else
{
    var runtime = compiler.Runtime;

    var zsModule = runtime.ImportIR(mainModule);

    ZSharp.IR.Function? main = null;
    foreach (var function in mainModule.Functions)
    {
        if (function.Name == "main" && function.Signature.Length == 0)
            if (main is not null) throw new Exception("Multiple main functions found!");
            else main = function;
    }

    if (main is not null)
    {
        var result = runtime.Call(main);

        if (result is null)
            Console.WriteLine("Main (Void)");
        else if (result is ZSharp.VM.ZSString stringResult)
            if (stringResult.Value == "ok") Environment.Exit(0);
            else if (stringResult.Value == "fail") Environment.Exit(1);
            else Console.WriteLine("Main (String): " + stringResult.Value);
        else if (result is ZSharp.VM.ZSInt32 int32Result)
            Environment.Exit(int32Result.Value);
        else Console.WriteLine($"Main ({result.Type}): " + result);
    }
    else Console.WriteLine("No main function found!");
}

Console.WriteLine();
Console.WriteLine("Press any key to exit...");
Console.ReadKey();
