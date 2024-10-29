using CommonZ.Utils;
using System.Diagnostics.CodeAnalysis;

namespace ZSharp.Compiler
{
    internal sealed class ObjectCache()
    {
        private readonly Cache<IR.IRObject, CGObject> cgCache = [];
        private readonly Cache<CGObject, IR.IRObject> irCache = [];

        public ObjectCache(ObjectCache parent)
            : this()
        {
            cgCache = new()
            {
                Parent = parent.cgCache,
            };
            irCache = new()
            {
                Parent = parent.irCache,
            };
        }

        public CGObject? CG(IR.IRObject ir)
            => cgCache.Cache(ir);

        public bool CG(IR.IRObject ir, [NotNullWhen(true)] out CGObject? cg)
            => cgCache.Cache(ir, out cg);

        public CGObject CG(IR.IRObject ir, CGObject cg)
            => cgCache.Cache(ir, cg);

        public IR.IRObject? IR(CGObject cg)
            => irCache.Cache(cg);

        public bool IR(CGObject cg, [NotNullWhen(true)] out IR.IRObject? ir)
            => irCache.Cache(cg, out ir);

        public IR.IRObject IR(CGObject cg, IR.IRObject ir)
            => irCache.Cache(cg, ir);
    }
}
