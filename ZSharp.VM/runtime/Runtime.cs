using ZSharp.IR;

namespace ZSharp.VM
{
    public sealed partial class Runtime
    {
        public RuntimeModule RuntimeModule { get; }

        public Runtime(RuntimeModule runtimeModule)
            : this(runtimeModule, new(runtimeModule)) { }

        public Runtime(RuntimeModule runtimeModule, TypeSystem typeSystem)
        {
            RuntimeModule = runtimeModule;
            TypeSystem = typeSystem;

            Initialize();
        }

        private void Initialize()
        {
            InitializeTypeSystem();
            InitializeSingletonValues();
        }
    }
}
