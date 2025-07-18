namespace ZSharp.Runtime
{
    public sealed partial class Runtime
    {
        public Runtime()
            : this(IR.RuntimeModule.Standard)
        {

        }

        public Runtime(IR.RuntimeModule runtimeModule)
        {
            RuntimeModule = runtimeModule;
            Loader = new(this);
        }
    }
}
