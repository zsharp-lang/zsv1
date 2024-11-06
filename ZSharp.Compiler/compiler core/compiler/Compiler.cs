using ZSharp.RAST;

namespace ZSharp.Compiler
{
    public sealed partial class Compiler
    {
        private readonly DocumentCompiler documentCompiler;
        private readonly ModuleCompiler moduleCompiler;

        public Compiler()
            : this(IR.RuntimeModule.Standard) { }

        public Compiler(IR.RuntimeModule runtimeModule)
            : this(new VM.Runtime(runtimeModule)) { }

        public Compiler(VM.Runtime runtime)
        {
            Context = new(new());

            ExpressionCompiler = new(this);
            LiteralCompiler = new(this);
            StatementCompiler = new(this);

            Runtime = runtime;

            Context.AddCompilerFor(documentCompiler = new(this));
            Context.AddCompilerFor(moduleCompiler = new(this));

            TypeSystem = new(this);

            Initialize();
        }

        public IR.Module CompileAsDocument(IEnumerable<RStatement> nodes)
            => documentCompiler.Compile(CreateImplicitModule(nodes)).IR!;

        public IR.Module CompileAsModule(IEnumerable<RStatement> nodes)
            => moduleCompiler.Compile(CreateImplicitModule(nodes)).IR!;

        private void Initialize()
        {
            InitializeTypeSystem();
            InitializeLiterals();
        }

        private static RModule CreateImplicitModule(IEnumerable<RStatement> nodes)
            => new(string.Empty)
            {
                Content = new RBlock(new(nodes))
            };
    }
}
