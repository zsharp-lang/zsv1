namespace ZSharp.Compiler
{
    public partial class IR(RuntimeModule runtimeModule)
    {
        public RuntimeModule RuntimeModule { get; } = runtimeModule;

        public IR Clone() => (IR)MemberwiseClone();
    }
}
