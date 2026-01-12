namespace ZSharp.Platform.Runtime.Loaders
{
    partial class ValueTypeLoader
    {
        private void LoadNestedType(IR.TypeDefinition type)
        {
            TypeLoaderHelper.LoadType(Loader, ILType, type);
        }
    }
}
