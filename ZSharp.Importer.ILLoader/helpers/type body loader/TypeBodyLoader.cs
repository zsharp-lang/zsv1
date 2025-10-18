namespace ZSharp.Importer.ILLoader
{
    internal sealed partial class TypeBodyLoader(ILLoader loader)
    {
        public ILLoader Loader { get; } = loader;
    }
}
