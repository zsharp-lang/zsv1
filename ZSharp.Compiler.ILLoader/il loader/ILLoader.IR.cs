namespace ZSharp.Compiler.ILLoader
{
    partial class ILLoader
    {
        public IR? IR { get; }

        public IR RequireIR()
            => IR ?? throw new InvalidOperationException("ILLoader was not initialized with a Runtime");
    }
}
