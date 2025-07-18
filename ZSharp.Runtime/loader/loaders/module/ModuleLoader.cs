namespace ZSharp.Runtime.Loaders
{
    internal sealed partial class ModuleLoader
        : LoaderBase
    {
        public Emit.AssemblyBuilder AssemblyBuilder { get; }

        public Emit.ModuleBuilder ILModule { get; }

        public Emit.TypeBuilder Globals { get; }

        public IR.Module IRModule { get; }

        public ModuleLoader(EmitLoader loader, IR.Module module)
            : base(loader)
        {
            IRModule = module;
            AssemblyBuilder = Emit.AssemblyBuilder.DefineDynamicAssembly(
                new IL.AssemblyName(IRModule.Name ?? throw new()),
                Emit.AssemblyBuilderAccess.RunAndCollect
            );
            ILModule = AssemblyBuilder.DefineDynamicModule(IRModule.Name);
            Globals = ILModule.DefineType("<Globals>");

            tasks = new(LoadAll);
        }
    }
}
