namespace ZSharp.Platform.Runtime.Loaders
{
    partial class InterfaceLoader
    {
        private void LoadNestedType(IR.TypeDefinition type)
        {
            TypeLoaderHelper.LoadType(Loader, ILType, type);
        }
    }
}
