namespace ZSharp.Platform.Runtime.Loaders
{
    partial class ModuleLoader
    {
        public void LoadType(IR.TypeDefinition type)
            => TypeLoaderHelper.LoadType(Loader, ILModule, type);
    }
}
