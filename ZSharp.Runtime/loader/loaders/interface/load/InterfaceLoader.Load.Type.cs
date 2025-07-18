namespace ZSharp.Runtime.Loaders
{
    partial class InterfaceLoader
    {
        private void LoadNestedType(IR.OOPType type)
        {
            TypeLoaderHelper.LoadType(Loader, ILType, type);
        }
    }
}
