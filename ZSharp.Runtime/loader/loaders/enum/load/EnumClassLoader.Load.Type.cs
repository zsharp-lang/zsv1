namespace ZSharp.Runtime.Loaders
{
    partial class EnumClassLoader
    {
        private void LoadNestedType(IR.OOPType type)
        {
            TypeLoaderHelper.LoadType(Loader, ILType, type);
        }
    }
}
