namespace ZSharp.Platform.Runtime
{
    partial class Runtime
    {
        private readonly Dictionary<IR.Module, IL.Module> _moduleCache = [];

        public void AddModule(IR.Module ir, IL.Module il)
            => _moduleCache.Add(ir, il);

        public void DelModule(IR.Module ir)
            => _moduleCache.Remove(ir);

        public void SetModule(IR.Module ir, IL.Module il)
            => _moduleCache[ir] = il;
    }
}
