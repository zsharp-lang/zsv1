namespace ZSharp.Platform.Runtime.Loaders
{
    partial class ClassLoader
    {
        private void LoadNestedType(IR.TypeDefinition type)
        {
            TypeLoaderHelper.LoadType(Loader, ILType, type);
        }
    }
}
