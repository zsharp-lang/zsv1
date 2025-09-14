namespace ZSharp.Platform.Runtime.Loaders
{
    partial class ClassLoader
    {
        private void LoadNestedType(IR.OOPType type)
        {
            TypeLoaderHelper.LoadType(Loader, ILType, type);
        }
    }
}
