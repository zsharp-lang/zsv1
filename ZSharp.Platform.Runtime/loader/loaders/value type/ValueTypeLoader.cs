namespace ZSharp.Platform.Runtime.Loaders
{
    internal sealed partial class ValueTypeLoader
        : LoaderBase
    {
        public required Emit.TypeBuilder ILType { get; init; }

        public required IR.ValueType IRType { get; init; }

        public ValueTypeLoader(EmitLoader loader)
            : base(loader)
        {
            
        }
    }
}
