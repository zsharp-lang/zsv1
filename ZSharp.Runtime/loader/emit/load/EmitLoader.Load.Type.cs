namespace ZSharp.Runtime.Loaders
{
    partial class EmitLoader
    {
        public Type LoadType(IR.OOPType type)
            => TypeLoaderHelper.LoadType(this, StandaloneModule, type);
    }
}
