using ZSharp.Compiler;

namespace ZSharp.Interpreter
{
    public class Interpreter
    {
        public Runtime.NET.Runtime HostLoader => Runtime;

        public Runtime.NET.Runtime Runtime { get; }

        public IR.RuntimeModule RuntimeModule { get; }

        public Compiler.Compiler Compiler { get; init; }

        public Compiler.IRLoader.IRLoader CompilerIRLoader { get; }

        public ZSSourceCompiler.ZSSourceCompiler SourceCompiler { get; init; }

        public Parser.ZSharpParser Parser { get; init; }

        public Interpreter(IR.RuntimeModule? runtimeModule = null)
        {
            RuntimeModule = runtimeModule ?? IR.RuntimeModule.Standard;

            Compiler = new(RuntimeModule);
            Runtime = new(RuntimeModule);

            new Ops(Compiler);

            CompilerIRLoader = new(Compiler);
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

        public Result<CompilerObject, string> Evaluate(Expression expression)
        {
            if (
                SourceCompiler.CompileNode(expression)
                .When(out var result)
                .IsError
            ) return Result<CompilerObject, string>.Error(string.Empty);

            return Evaluate(result!);
        }

        public Result<CompilerObject, string> Evaluate(CompilerObject @object)
        {
            if (
                Compiler.IR.CompileCode(@object)
                .When(out var irCode)
                .Error(out var error)
            )
                return Result<CompilerObject, string>.Ok(@object);

            var result = Runtime.Evaluator.EvaluateCT(irCode ?? throw new(), Compiler);

            if (irCode.IsVoid)
                return Result<CompilerObject, string>
                    .Ok(new Objects.RawCode(new()));

            if (result is null)
                return Result<CompilerObject, string>
                    .Ok(@object);
                // TODO: should actually be an error!
                //return Result<CompilerObject, string> 
                //    .Error("Non-void expression evaluated to nothing");

            return Result<CompilerObject, string>.Ok(result);
        }
    }
}
