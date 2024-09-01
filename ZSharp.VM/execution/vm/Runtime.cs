using ZSharp.IR;

namespace ZSharp.VM
{
    public sealed partial class Runtime
    {
        public RuntimeModule RuntimeModule { get; }

        public Runtime(RuntimeModule runtimeModule)
        {
            this.RuntimeModule = runtimeModule;
            this.assembler = new(this);
        }
    }
}
