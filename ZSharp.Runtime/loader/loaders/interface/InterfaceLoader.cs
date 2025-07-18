namespace ZSharp.Runtime.Loaders
{
    internal sealed partial class InterfaceLoader
        : LoaderBase
    {
        public required Emit.TypeBuilder ILType { get; init; }

        public required IR.Interface IRType { get; init; }

        public InterfaceLoader(EmitLoader loader)
            : base(loader)
        {
            
        }
    }
}
