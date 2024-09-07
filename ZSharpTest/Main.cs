using ZSharp.Compiler;
using ZSharp.Parser;
using ZSharp.Text;
using ZSharp.Tokenizer;


#region RAST

///*
// * The code below represents the RAST of the following Z# code:
// * 
// * let stdIO = "std:io";
// * import { print } from stdIO;
// * 
// * fun id(x: string): string {
// *   return x;
// * }
// * 
// * let message = id("Hello, World!");
// * print(message);
// * 
// * fun foo(x: string): void {
// *   print(x);
// * }
// * 
// * foo("Hello, foo!");
// */


////RId stdIO;
////RId print;
////RId id;
////RId id_x;
////RId message;
////RId foo_x;
////RId foo;
//var rastNodes = new RStatement[]
//{
//    //// let stdIO = "std:io";
//    //new RLetStatement(
//    //    new RNamePattern(stdIO = new("stdIO")),
//    //    null,
//    //    RLiteral.String("std:io")
//    //),

//    new RExpressionStatement(
//        new RLetDefinition(
//            "myGlobal", 
//            null,
//            value: RLiteral.String("Hello, Global!")
//        )
//    ),

//    //// import { print } from stdIO;
//    //new RImport(RLiteral.String("std:io"))
//    //{
//    //    Targets = [
//    //        new(print = new("print"))
//    //    ]
//    //},

//    //new RExpressionStatement(
//    //    new RFunction(
//    //        id = new("id"),
//    //        new()
//    //        {
//    //            Args = [
//    //                new(id_x = new("x"), globalString, null)
//    //            ]
//    //        })
//    //    {
//    //        ReturnType = globalString,
//    //        Body = new RReturn(id_x)
//    //    }
//    //),

//    //// let message = "Hello, World!";
//    //new RLetStatement(
//    //    new RNamePattern(message = new("message")),
//    //    null, //globals["string"],
//    //    new RCall(
//    //        id,
//    //        [
//    //            new(RLiteral.String("Hello, World!"))
//    //        ]
//    //    )
//    //),

//    //// print(message);
//    //new RExpressionStatement(
//    //    new RCall(print,
//    //    [
//    //        new(message)
//    //    ])
//    //),

//    //// print("Hello");
//    //new RExpressionStatement(
//    //    new RCall(print,
//    //    [
//    //        new(RLiteral.String("Hello"))
//    //    ])
//    //),

//    //// fun foo(x: string): void { print(x); }
//    //new RExpressionStatement(
//    //    new RFunction(
//    //        foo = new("foo"),
//    //        new() {
//    //            Args =
//    //            [
//    //                new(foo_x = new("x"), globalString, null)
//    //            ]
//    //        })
//    //    {
//    //        ReturnType = globalVoid,
//    //        Body = new RReturn(
//    //            new RCall(print,
//    //            [
//    //                new(foo_x)
//    //            ])
//    //        )
//    //    }
//    //),

//    //// foo("Hello, foo!");
//    //new RExpressionStatement(
//    //    new RCall(foo,
//    //    [
//    //        new(RLiteral.String("Hello, foo!"))
//    //    ])
//    //)
//};

#endregion


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
        TokenType.Identifier,
        token => new ZSharp.AST.IdentifierExpression(token.Value)
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
//compiler.Initialize();

var standardModule = new ZSharp.CT.StandardLibrary.StandardModule();
compiler.Runtime.AddInternalModule(standardModule);
compiler.Context.CurrentScope.Cache("print", standardModule.Print);

//var cgCode = compiler.CompileCG(rastNodes);
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
            Console.WriteLine("Main (String): " + stringResult.Value);
        else Console.WriteLine($"Main ({result.Type}): " + result);
    }
    else Console.WriteLine("No main function found!");
}

Console.WriteLine();
Console.WriteLine("Press any key to exit...");
Console.ReadKey();
