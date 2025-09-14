namespace ZSharp.Importer.ILLoader
{
    partial class ILLoader
    {
        public Compiler.IR? IR { get; }

        public Compiler.IR RequireIR()
            => IR ?? throw new InvalidOperationException("ILLoader was not initialized with a Runtime");
    }
}
