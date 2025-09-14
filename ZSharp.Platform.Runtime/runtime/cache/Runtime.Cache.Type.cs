using CommonZ.Utils;

namespace ZSharp.Platform.Runtime
{
    partial class Runtime
    {
        private Cache<IR.IType, Type> _typeCache = [];
        private readonly Dictionary<IR.OOPType, Type> _typeDefCache = [];

        public Cache<IR.IType, Type> NewTypeContext()
            => new() { Parent = _typeCache };

        public ContextManager TypeContext(Cache<IR.IType, Type>? context = null)
        {
            context ??= NewTypeContext();

            (_typeCache, context) = (context, _typeCache);

            return new(() => _typeCache = context);
        }

        public void AddType(IR.IType ir, Type il)
            => SetType(ir, il);

        public void DelType(IR.IType ir)
            => _typeCache.Uncache(ir);

        public void SetType(IR.IType ir, Type il)
            => _typeCache.Cache(ir, il);

        public void AddTypeDefinition(IR.OOPType ir, Type il)
            => _typeDefCache.Add(ir, il);

        public void DelTypeDefinition(IR.OOPType ir)
            => _typeDefCache.Remove(ir);

        public void SetTypeDefinition(IR.OOPType ir, Type il)
            => _typeDefCache[ir] = il;
    }
}
