namespace ZSharp.Importer.ILLoader.Objects
{
    partial class TypeAsModule
    {
        public ILLoader Loader { get; }

        internal ModuleBodyLoader BodyLoader { get; }

        internal ILazyMemberLoader LazyLoader { get; }
    }
}
