using System.Reflection;

namespace ZSharp.Importer.ILLoader.Objects
{
    internal sealed partial class Module
        : CompilerObject
    {
        public string Name => IL.Name;

        public Module(IL.Module il, ILLoader loader)
        {
            IL = il;
            Loader = loader;

            LazyLoader = new LazyMemberLoader()
            {
                Container = this,
                Loader = new ModuleBodyLoader(loader)
            };

            Prepare.PrepareModule(this, loader);
        }
    }
}
