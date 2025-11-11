namespace ZSharp.Importer.ILLoader.Objects
{
    public sealed partial class GenericInterface
        : CompilerObject
    {
        public GenericInterface(Type il, ILLoader loader)
        {
            IL = il;
            Loader = loader;
        }
    }
}
