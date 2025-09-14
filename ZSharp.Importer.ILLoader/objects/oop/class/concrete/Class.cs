namespace ZSharp.Importer.ILLoader.Objects
{
    public sealed partial class Class
        : CompilerObject
    {
        public Class(Type il, ILLoader loader)
        {
            IL = il;
            Loader = loader;
        }
    }
}
