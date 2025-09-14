namespace ZSharp.Platform.Runtime
{
    partial class Runtime
    {
        private readonly Dictionary<IR.Field, IL.FieldInfo> _fieldCache = [];

        public void AddField(IR.Field ir, IL.FieldInfo il)
            => _fieldCache.Add(ir, il);

        public void DelField(IR.Field ir)
            => _fieldCache.Remove(ir);

        public void SetField(IR.Field ir, IL.FieldInfo il)
            => _fieldCache[ir] = il;
    }
}
