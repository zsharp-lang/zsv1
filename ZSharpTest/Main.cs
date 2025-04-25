using System.Text;
using System.Text.RegularExpressions;
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
    expressionParser.InfixL("**", 80);

    expressionParser.InfixL("==", 30);
    expressionParser.InfixL("!=", 30);

    expressionParser.Led(TokenType.LParen, LangParser.ParseCallExpression, 100);
    expressionParser.Led(TokenType.LBracket, LangParser.ParseIndexExpression, 100);
    expressionParser.Nud(TokenType.LBracket, LangParser.ParseArrayLiteral);
    expressionParser.Led(".", LangParser.ParseMemberAccess, 150);

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


#region Compilation

var interpreter = new Interpreter();

interpreter.SourceCompiler.StringImporter.Importers.Add(
    "net",
    new ZSharp.DotNETImporter(interpreter)
);

new Referencing(interpreter.Compiler);
new OOP(interpreter.Compiler);

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

foreach (var moduleIR in new[] { moduleIR_standardIO, moduleIR_standardMath })
    if (moduleIR.HasFunctions)
        foreach (var function in moduleIR.Functions)
        {
            if (function.Name is null || function.Name == string.Empty)
                continue;

            var match = Regex.Match(function.Name, @"^_?(?<OP>[+\-*=?&^%$#@!<>|~]+)_?$");
            if (match.Success)
            {
                var op = match.Groups["OP"].Value;
                if (!interpreter.SourceCompiler.Operators.Cache(op, out var group))
                    group = interpreter.SourceCompiler.Operators.Cache(op, new ZSharp.Objects.OverloadGroup(op));

                if (group is not ZSharp.Objects.OverloadGroup overloadGroup)
                    throw new Exception("Invalid overload group!");

                overloadGroup.Overloads.Add(interpreter.CompilerIRLoader.Import(function));
            }
        }

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

    //foreach (var type in mainModuleIL.GetTypes())
    //    DecompileType(type);

    //foreach (var method in mainModuleIL.GetMethods())
    //    Decompile(method);

    var mainMethod = mainModuleGlobals.GetMethod("main", []);

    mainMethod?.Invoke(null, null);
}

#endregion

Console.WriteLine();

Console.WriteLine();
Console.WriteLine("Press any key to exit...");
Console.ReadKey();


static void DecompileType(Type type)
{
    Console.WriteLine("========== Type: " + type.Name + " ==========");

    foreach (var method in type.GetMethods())
    {
        if (method.DeclaringType != type)
            continue;

        Decompile(method);
    }

    Console.WriteLine("========== Type ==========");
}


static void Decompile(System.Reflection.MethodBase method)
{
    StringBuilder signatureBuilder = new();
    foreach (var parameter in method.GetParameters())
    {
        if (signatureBuilder.Length > 0)
            signatureBuilder.Append(", ");
        signatureBuilder.Append(parameter.ParameterType.Name);
    }

    string modifier = string.Empty;
    if (method.IsStatic)
        modifier = "static ";
    else if (method.IsVirtual)
        modifier = "virtual ";
    else if (method.IsAbstract)
        modifier = "abstract ";
    else modifier = "instance ";

    Console.WriteLine($"========== Disassmebly: {modifier}{method.Name}({signatureBuilder}) ==========");

    foreach (var instruction in Mono.Reflection.Disassembler.GetInstructions(method))
    {
        Console.WriteLine(instruction);
    }

    Console.WriteLine("========== Disassmebly ==========");
}
