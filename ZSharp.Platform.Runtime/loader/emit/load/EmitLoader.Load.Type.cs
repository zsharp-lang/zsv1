namespace ZSharp.Platform.Runtime.Loaders
{
    partial class EmitLoader
    {
        public Type LoadType(IR.TypeDefinition type)
            => TypeLoaderHelper.LoadType(this, StandaloneModule, type);
    }
}
