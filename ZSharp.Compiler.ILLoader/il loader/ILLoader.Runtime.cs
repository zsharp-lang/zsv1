namespace ZSharp.Compiler.ILLoader
{
    partial class ILLoader
    {
        public Runtime.Runtime? Runtime { get; }

        public Runtime.Runtime RequireRuntime()
            => Runtime ?? throw new InvalidOperationException("ILLoader was not initialized with a Runtime");
    }
}
