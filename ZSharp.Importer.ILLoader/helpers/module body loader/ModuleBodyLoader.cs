namespace ZSharp.Importer.ILLoader
{
    internal sealed partial class ModuleBodyLoader(ILLoader loader)
    {
        public ILLoader Loader { get; } = loader;
    }
}
