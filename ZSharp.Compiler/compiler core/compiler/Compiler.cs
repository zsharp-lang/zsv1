namespace ZSharp.Compiler
{
    public sealed partial class Compiler
    {
        public Compiler()
            : this(IR.RuntimeModule.Standard) { }

        public Compiler(IR.RuntimeModule runtimeModule)
        {
            RuntimeModule = runtimeModule;

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
