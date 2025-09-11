namespace ZSharp.Runtime
{
    partial class Runtime
    {
        private IL.Module LoadModule(IR.Module module)
            => Loader.LoadModule(module);
    }
}
