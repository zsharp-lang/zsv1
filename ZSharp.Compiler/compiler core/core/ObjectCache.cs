using CommonZ.Utils;
using System.Diagnostics.CodeAnalysis;

namespace ZSharp.Compiler
{
    internal sealed class ObjectCache()
    {
        private readonly Cache<IR.IRObject, CompilerObject> cgCache = [];
        private readonly Cache<CompilerObject, IR.IRObject> irCache = [];

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

        public CompilerObject? CG(IR.IRObject ir)
            => cgCache.Cache(ir);

        public bool CG(IR.IRObject ir, [NotNullWhen(true)] out CompilerObject? cg)
            => cgCache.Cache(ir, out cg);

        public bool CG<T>(IR.IRObject ir, [NotNullWhen(true)] out T? cg)
            where T : CompilerObject
            => cgCache.Cache(ir, out cg);


        public CompilerObject CG(IR.IRObject ir, CompilerObject cg)
            => CG<CompilerObject>(ir, cg);

        public T CG<T>(IR.IRObject ir, T cg)
            where T : CompilerObject
        {
            cgCache.Cache(ir, cg);
            irCache.Cache(cg, ir);
            return cg;
        }

        public IR.IRObject? IR(CompilerObject cg)
            => irCache.Cache(cg);

        public bool IR(CompilerObject cg, [NotNullWhen(true)] out IR.IRObject? ir)
            => irCache.Cache(cg, out ir);

        public bool IR<T>(CompilerObject cg, [NotNullWhen(true)] out T? ir)
            where T : IR.IRObject
            => irCache.Cache(cg, out ir);

        public IR.IRObject IR(CompilerObject cg, IR.IRObject ir)
            => IR<IR.IRObject>(cg, ir);

        public T IR<T>(CompilerObject cg, T ir)
            where T : IR.IRObject
        {
            irCache.Cache(cg, ir);
            cgCache.Cache(ir, cg);
            return ir;
        }
    }
}
