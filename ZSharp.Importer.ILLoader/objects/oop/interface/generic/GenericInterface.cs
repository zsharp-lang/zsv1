namespace ZSharp.Importer.ILLoader.Objects
{
    public sealed partial class GenericInterface
        : CompilerObject
    {
        public GenericInterface(Type il, ILLoader loader)
        {
            IL = il;
            Loader = loader;

            //LazyLoader = new LazyMemberLoader()
            //{
            //    Container = this,
            //    Loader = new TypeBodyLoader(loader)
            //};

            //Prepare.PrepareType(this, loader);
        }
    }
}
