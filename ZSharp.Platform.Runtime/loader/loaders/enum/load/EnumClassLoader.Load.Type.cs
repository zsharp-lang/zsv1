namespace ZSharp.Platform.Runtime.Loaders
{
    partial class EnumClassLoader
    {
        private void LoadNestedType(IR.TypeDefinition type)
        {
            TypeLoaderHelper.LoadType(Loader, ILType, type);
        }
    }
}
