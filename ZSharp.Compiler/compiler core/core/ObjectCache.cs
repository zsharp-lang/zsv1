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

        public bool CG<T>(IR.IRObject ir, [NotNullWhen(true)] out T? cg)
            where T : CGObject
            => cgCache.Cache(ir, out cg);


        public CGObject CG(IR.IRObject ir, CGObject cg)
            => CG<CGObject>(ir, cg);

        public T CG<T>(IR.IRObject ir, T cg)
            where T : CGObject
        {
            cgCache.Cache(ir, cg);
            irCache.Cache(cg, ir);
            return cg;
        }

        public IR.IRObject? IR(CGObject cg)
            => irCache.Cache(cg);

        public bool IR(CGObject cg, [NotNullWhen(true)] out IR.IRObject? ir)
            => irCache.Cache(cg, out ir);

        public bool IR<T>(CGObject cg, [NotNullWhen(true)] out T? ir)
            where T : IR.IRObject
            => irCache.Cache(cg, out ir);

        public IR.IRObject IR(CGObject cg, IR.IRObject ir)
            => IR<IR.IRObject>(cg, ir);

        public T IR<T>(CGObject cg, T ir)
            where T : IR.IRObject
        {
            irCache.Cache(cg, ir);
            cgCache.Cache(ir, cg);
            return ir;
        }
    }
}
