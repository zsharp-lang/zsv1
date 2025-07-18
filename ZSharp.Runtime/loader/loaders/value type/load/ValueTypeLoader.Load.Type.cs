namespace ZSharp.Runtime.Loaders
{
    partial class ValueTypeLoader
    {
        private void LoadNestedType(IR.OOPType type)
        {
            TypeLoaderHelper.LoadType(Loader, ILType, type);
        }
    }
}
