namespace ZSharp.Platform.Runtime.Loaders
{
    partial class EmitLoader
    {
        public IL.Module LoadModule(IR.Module module)
            => new ModuleLoader(this, module).Load();
    }
}
