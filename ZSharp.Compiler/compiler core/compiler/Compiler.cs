namespace ZSharp.Compiler
{
    public sealed partial class Compiler
    {
        public Compiler()
            : this(ZSharp.IR.RuntimeModule.Standard) { }

        public Compiler(ZSharp.IR.RuntimeModule runtimeModule)
        {
            RuntimeModule = runtimeModule;

            CG = new(this);
            IR = new(this);
            TypeSystem = new(this);

            Initialize();
        }

        private void Initialize()
        {
            InitializeTypeSystem();
            InitializeLiterals();
            InitializeFeatures();
            InitializeEvaluators();
        }
    }
}
