using ZSharp.Compiler;
using ZSharp.Interpreter;
using ZSharp.Parser;
using ZSharp.Text;
using ZSharp.Tokenizer;

string fileName = args.Length == 0 ? "test.zs" : args[0];
string filePath = Path.GetFullPath(fileName);

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
    expressionParser.InfixL("**", 80);

    expressionParser.Led(TokenType.LParen, LangParser.ParseCallExpression, 100);
    expressionParser.Led(".", LangParser.ParseMemberAccess, 150);

    expressionParser.Separator(TokenType.Comma);
    expressionParser.Separator(TokenType.RParen);
    expressionParser.Separator(TokenType.Semicolon);

    expressionParser.AddKeywordParser(
        LangParser.Keywords.While,
        LangParser.ParseWhileExpression
    );

    statementParser.AddKeywordParser(
        LangParser.Keywords.While,
        Utils.ExpressionStatement(LangParser.ParseWhileExpression, semicolon: false)
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


#region Compilation

var interpreter = new Interpreter();
ZSharp.Runtime.NET.Runtime runtime = new(interpreter);

interpreter.Runtime = runtime;
interpreter.HostLoader = runtime;

ZS.RuntimeAPI.Fields_Globals.runtime = runtime;
runtime.Hooks.GetObject = ZSharp.Runtime.NET.Utils.GetMethod(ZS.RuntimeAPI.Impl_Globals.GetObject);

var moduleIL_standardIO = typeof(Standard.IO.Impl_Globals).Module;
var moduleIR_standardIO = interpreter.HostLoader.Import(moduleIL_standardIO);
var moduleCO_standardIO = interpreter.CompilerIRLoader.Import(moduleIR_standardIO);

interpreter.Compiler.TypeSystem.String.ToString = interpreter.CompilerIRLoader.Import(
    runtime.Import(
        ZSharp.Runtime.NET.Utils.GetMethod(Standard.IO.Impl_Globals.ToString)
    )
);

interpreter.Compiler.TypeSystem.Int32.Members["parse"] = interpreter.CompilerIRLoader.Import(
    runtime.Import(
        ZSharp.Runtime.NET.Utils.GetMethod(Standard.IO.Impl_Globals.ParseInt32)
    )
);

interpreter.SourceCompiler.StandardLibraryImporter.Libraries.Add("io", moduleCO_standardIO);

var moduleIL_standardMath = typeof(Standard.Math.Impl_Globals).Module;
var moduleIR_standardMath = interpreter.HostLoader.Import(moduleIL_standardMath);
var moduleCO_standardMath = interpreter.CompilerIRLoader.Import(moduleIR_standardMath);

interpreter.SourceCompiler.StandardLibraryImporter.Libraries.Add("math", moduleCO_standardMath);

interpreter.SourceCompiler.Operators.Cache(
    "+",
    interpreter.CompilerIRLoader.Import(
        runtime.Import(
            ZSharp.Runtime.NET.Utils.GetMethod(Standard.IO.Impl_Globals.Concat)
        )
    )
);

interpreter.SourceCompiler.Operators.Cache(
    "<",
    interpreter.CompilerIRLoader.Import(
        runtime.Import(
            ZSharp.Runtime.NET.Utils.GetMethod(Standard.Math.Impl_Globals.LessThan)
        )
    )
);

interpreter.SourceCompiler.Operators.Cache(
    "-",
    interpreter.CompilerIRLoader.Import(
        runtime.Import(
            ZSharp.Runtime.NET.Utils.GetMethod(Standard.Math.Impl_Globals.Subtract)
        )
    )
);

//var moduleIL_compilerAPI = typeof(ZS.CompilerAPI.Impl_Globals).Module;
//var moduleIR_compilerAPI = interpreter.HostLoader.Import(moduleIL_compilerAPI);
//var moduleCO_compilerAPI = interpreter.CompilerIRLoader.Import(moduleIR_compilerAPI);

//interpreter.SourceCompiler.ZSImporter.Libraries.Add("compiler", moduleCO_compilerAPI);

var document = interpreter.CompileDocument(documentNode, filePath);

Console.WriteLine($"Compilation finished with {interpreter.Compiler.Log.Logs.Count(l => l.Level == LogLevel.Error)} errors!");

foreach (var log in interpreter.Compiler.Log.Logs)
    Console.WriteLine(log);

ZSharp.Objects.Module? mainModule = document.Content.FirstOrDefault(
    item => item is ZSharp.Objects.Module module && module.Name == "Program"
) as ZSharp.Objects.Module;

if (mainModule is not null)
{
    var mainModuleIR = 
        interpreter.Compiler.CompileIRObject<ZSharp.IR.Module, ZSharp.IR.Module>(mainModule, null) ?? throw new();

    var mainModuleIL = runtime.Import(mainModuleIR);
    var mainModuleGlobals = mainModuleIL.GetType("<Globals>") ?? throw new();

    var mainMethod = mainModuleGlobals.GetMethod("main", []);

    if (mainMethod is not null)
        Decompile(mainMethod);

    mainMethod?.Invoke(null, null);
}

#endregion

Console.WriteLine();

Console.WriteLine();
Console.WriteLine("Press any key to exit...");
Console.ReadKey();


static void Decompile(System.Reflection.MethodBase method)
{
    Console.WriteLine("========== Disassmebly: " + method.Name + " ==========");

    foreach (var instruction in Mono.Reflection.Disassembler.GetInstructions(method))
    {
        Console.WriteLine(instruction);
    }

    Console.WriteLine("========== Disassmebly ==========");
}
