using ZSharp.Compiler;

namespace ZSharp.Interpreter
{
    public sealed partial class Interpreter
    {
        //public Runtime.NET.Runtime Runtime { get; }

        public IR.RuntimeModule RuntimeModule { get; }

        public Compiler.Compiler Compiler { get; }

        public IRCompiler.Compiler IRCompiler { get; }

        public ZSSourceCompiler.ZSSourceCompiler SourceCompiler { get; init; }

        public Parser.ZSharpParser Parser { get; init; }

        public Interpreter(IR.RuntimeModule? runtimeModule = null)
        {
            RuntimeModule = runtimeModule ?? IR.RuntimeModule.Standard;

            Compiler = new(RuntimeModule);
            Runtime = new(RuntimeModule);
            IRCompiler = new(RuntimeModule);
            ILLoader = new(Compiler);

            new Ops(Compiler);

            SourceCompiler = new(this);

            Parser = new();
        }

        public ZSSourceCompiler.Document CompileFile(string path)
        {
            AST.Document document;

            using (var reader = new StreamReader(path))
                document = Parser.Parse(new(Tokenizer.Tokenizer.Tokenize(new(reader))));

            return CompileDocument(document, path);
        }

        public ZSSourceCompiler.Document CompileDocument(AST.Document document, string path)
        {
            return SourceCompiler.CreateDocumentCompiler(document, path).Compile();
        }

        public Result<object?, string> Evaluate(Expression expression)
        {
            if (
                SourceCompiler.CompileNode(expression)
                .When(out var result)
                .IsError
            ) return Result<object?, string>.Error(string.Empty);

            return Evaluate(result!);
        }

        public Result<object?, string> Evaluate(CompilerObject @object)
        {
            if (
                Compiler.IR.CompileCode(@object)
                .When(out var irCode)
                .Error(out var error)
            )
                return Result<object?, string>.Ok(@object);

            var type = irCode!.IsVoid ? RuntimeModule.TypeSystem.Void : IRCompiler.CompileType(irCode.RequireValueType()).Unwrap();

            var result = Runtime.Evaluate(irCode.Instructions, type);

            if (irCode.IsVoid)
                return Result<object?, string>
                    .Ok(new Objects.RawCode(new()));

            if (result is null)
                return Result<object?, string>
                    .Ok(@object);
                // TODO: should actually be an error!
                //return Result<CompilerObject, string> 
                //    .Error("Non-void expression evaluated to nothing");

            return Result<object?, string>.Ok(result);
        }
    }
}
