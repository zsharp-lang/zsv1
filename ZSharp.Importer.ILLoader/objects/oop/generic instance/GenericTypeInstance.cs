namespace ZSharp.Importer.ILLoader.Objects
{
    public sealed partial class GenericTypeInstance
        : CompilerObject
    {
        public GenericTypeInstance(Type il, ILLoader loader)
        {
            IL = il;
            Loader = loader;

            GenericArguments = [.. il.GetGenericArguments().Select(arg => loader.LoadType(arg))];
            Definition = loader.LoadType(il.GetGenericTypeDefinition());

            //LazyLoader = new LazyMemberLoader()
            //{
            //    Container = this,
            //    Loader = new TypeBodyLoader(loader)
            //};

            //Prepare.PrepareType(this, loader);
        }
    }
}
