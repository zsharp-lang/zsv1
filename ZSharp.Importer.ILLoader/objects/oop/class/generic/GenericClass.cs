namespace ZSharp.Importer.ILLoader.Objects
{
    public sealed partial class GenericClass
        : CompilerObject
    {
        public GenericClass(Type il, ILLoader loader)
        {
            IL = il;
            Loader = loader;
        }
    }
}
