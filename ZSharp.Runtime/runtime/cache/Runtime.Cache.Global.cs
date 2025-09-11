namespace ZSharp.Runtime
{
    partial class Runtime
    {
        private readonly Dictionary<IR.Global, IL.FieldInfo> _globalCache = [];

        public void AddGlobal(IR.Global ir, IL.FieldInfo il)
            => _globalCache.Add(ir, il);

        public void DelGlobal(IR.Global ir)
            => _globalCache.Remove(ir);

        public void SetGlobal(IR.Global ir, IL.FieldInfo il)
            => _globalCache[ir] = il;
    }
}
