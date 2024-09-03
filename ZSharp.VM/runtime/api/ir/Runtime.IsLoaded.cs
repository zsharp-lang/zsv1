namespace ZSharp.VM
{
    public sealed partial class Runtime
    {
        public bool IsLoaded(IR.Module module)
            => irMap.Contains(module);
    }
}
