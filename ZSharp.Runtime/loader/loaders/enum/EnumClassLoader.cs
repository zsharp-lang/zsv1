namespace ZSharp.Runtime.Loaders
{
    internal sealed partial class EnumClassLoader
        : LoaderBase
    {
        public required Emit.TypeBuilder ILType { get; init; }

        public required IR.EnumClass IRType { get; init; }

        public EnumClassLoader(EmitLoader loader)
            : base(loader)
        {
            
        }
    }
}
