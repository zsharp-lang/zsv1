namespace ZSharp.Runtime.Loaders
{
    partial class ModuleLoader
    {
        public void LoadType(IR.OOPType type)
            => TypeLoaderHelper.LoadType(Loader, ILModule, type);
    }
}
