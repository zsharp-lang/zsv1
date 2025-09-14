namespace ZSharp.Importer.ILLoader
{
    partial class ILLoader
    {
        public Platform.Runtime.Runtime? Runtime { get; }

        public Platform.Runtime.Runtime RequireRuntime()
            => Runtime ?? throw new InvalidOperationException("ILLoader was not initialized with a Runtime");
    }
}
