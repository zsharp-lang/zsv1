using System.Reflection;

namespace ZSharp.Importer.ILLoader.Objects
{
    internal sealed partial class TypeAsModule
        : CompilerObject
    {
        public string Name => IL.Name;

        public TypeAsModule(Type il, ILLoader loader)
        {
            IL = il;
            Loader = loader;
            LazyLoader = new LazyMemberLoader()
            {
                Container = this,
                Loader = new ModuleBodyLoader(loader)
            };

            if (il.GetCustomAttribute<SetNamespaceAttribute>() is SetNamespaceAttribute setNamespace)
                RootNamespace = setNamespace.Name;
            else RootNamespace = string.Empty;

            Prepare.PrepareTypeAsModule(this, loader);
        }
    }
}
