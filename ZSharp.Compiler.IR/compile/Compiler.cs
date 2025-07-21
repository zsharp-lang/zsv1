namespace ZSharp.IRCompiler
{
    public sealed partial class Compiler(IR.RuntimeModule runtimeModule)
    {
        public IR.RuntimeModule RuntimeModule { get; } = runtimeModule;
    }
}
