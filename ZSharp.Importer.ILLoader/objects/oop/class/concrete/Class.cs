namespace ZSharp.Importer.ILLoader.Objects
{
    internal sealed partial class Class
        : CompilerObject
    {
        public Class(Type il, ILLoader loader)
        {
            IL = il;
            Loader = loader;

            LazyLoader = new LazyMemberLoader()
            {
                Container = this,
                Loader = new TypeBodyLoader(loader)
            };

            Prepare.PrepareType(this, loader);
        }
    }
}
