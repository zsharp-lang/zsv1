namespace ZSharp.Runtime.Loaders
{
    internal sealed partial class ClassLoader
        : LoaderBase
    {
        public required Emit.TypeBuilder ILType { get; init; }

        public required IR.Class IRType { get; init; }

        public ClassLoader(EmitLoader loader)
            : base(loader)
        {
            
        }
    }
}
