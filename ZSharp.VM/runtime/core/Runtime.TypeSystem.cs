namespace ZSharp.VM
{
    public sealed partial class Runtime
    {
        public TypeSystem TypeSystem { get; }

        private void InitializeTypeSystem()
        {
            irMap.Cache(RuntimeModule.TypeSystem.Type, TypeSystem.Type);

            irMap.Cache(RuntimeModule.TypeSystem.String, TypeSystem.String);
            irMap.Cache(RuntimeModule.TypeSystem.Void, TypeSystem.Void);
        }
    }
}
