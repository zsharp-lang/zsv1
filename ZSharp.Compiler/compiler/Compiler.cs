namespace ZSharp.Compiler
{
    public sealed partial class Compiler(ZSharp.IR.RuntimeModule runtimeModule)
    {
        public ZSharp.IR.RuntimeModule RuntimeModule { get; } = runtimeModule;

        public Compiler()
            : this(ZSharp.IR.RuntimeModule.Standard) { }
    }
}
