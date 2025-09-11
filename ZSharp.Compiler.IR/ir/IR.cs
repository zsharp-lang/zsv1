namespace ZSharp.Compiler
{
    public sealed partial class IR(ZSharp.IR.RuntimeModule runtimeModule)
    {
        public ZSharp.IR.RuntimeModule RuntimeModule { get; } = runtimeModule;
    }
}
