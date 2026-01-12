namespace ZSharp.Compiler
{
    public partial struct IR(RuntimeModule runtimeModule)
    {
        public RuntimeModule RuntimeModule { get; } = runtimeModule;
    }
}
