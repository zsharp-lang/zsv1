namespace ZSharp.Runtime
{
    partial class Runtime
    {
        private readonly Dictionary<IR.Function, IL.MethodBase> _functionCache = [];

        public void AddFunction(IR.Function ir, IL.MethodBase il)
            => _functionCache.Add(ir, il);

        public void DelFunction(IR.Function ir)
            => _functionCache.Remove(ir);

        public void SetFunction(IR.Function ir, IL.MethodBase il)
            => _functionCache[ir] = il;
    }
}
