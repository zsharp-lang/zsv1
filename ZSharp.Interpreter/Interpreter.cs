namespace ZSharp.Interpreter
{
    public class Interpreter
    {
        public IHostLoader HostLoader { get; set; } = null!;

        public IRuntime Runtime { get; set; } = null!;

        public IR.RuntimeModule RuntimeModule { get; }

        public Compiler.Compiler Compiler { get; init; }

        public Compiler.IRLoader.IRLoader CompilerIRLoader { get; }

        public ZSSourceCompiler.ZSSourceCompiler SourceCompiler { get; init; }

        public Parser.ZSharpParser Parser { get; init; }

        public Interpreter(IR.RuntimeModule? runtimeModule = null)
        {
            RuntimeModule = runtimeModule ?? IR.RuntimeModule.Standard;

            Compiler = new(RuntimeModule);

            CompilerIRLoader = new(Compiler);
            SourceCompiler = new(Compiler);

            Parser = new();

            Compiler.Evaluators.Add(new ZSSourceEvaluator(SourceCompiler));
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
    }
}
