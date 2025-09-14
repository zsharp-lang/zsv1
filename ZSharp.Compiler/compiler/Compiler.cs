namespace ZSharp.Compiler
{
    public sealed partial class Compiler
    {
        public ZSharp.IR.RuntimeModule RuntimeModule { get; }

        public Compiler()
            : this(ZSharp.IR.RuntimeModule.Standard) { }

        public Compiler(ZSharp.IR.RuntimeModule runtimeModule)
        {
            RuntimeModule = runtimeModule;

            cg = new();
            ir = new(runtimeModule);
            ts = new();
            overloading = new();
            reflection = new();
        }
    }
}
