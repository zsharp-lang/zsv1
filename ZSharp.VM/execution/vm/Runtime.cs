using ZSharp.IR;

namespace ZSharp.VM
{
    public sealed partial class Runtime(RuntimeModule runtimeModule)
    {
        public RuntimeModule RuntimeModule { get; } = runtimeModule;
    }
}
