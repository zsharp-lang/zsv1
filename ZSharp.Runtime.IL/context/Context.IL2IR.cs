using System.Diagnostics.CodeAnalysis;

namespace ZSharp.Runtime.NET
{
    partial class Context
    {
        public IR.Module? Cache(IL.Module il)
            => toIR.Modules.Cache(il);

        public bool Cache(IL.Module il, [NotNullWhen(true)] out IR.Module? ir)
            => toIR.Modules.Cache(il, out ir);

        public IR.Module Cache(IL.Module il, IR.Module ir)
        {
            if (!toIR.Modules.Contains(il))
                toIR.Modules.Cache(il, ir);

            if (!toIL.Modules.Contains(ir))
                toIL.Modules.Cache(ir, il);

            return ir;
        }

        public IR.IType? Cache(Type il)
            => toIR.Types.Cache(il);

        public bool Cache(Type il, [NotNullWhen(true)] out IR.IType? ir)
            => toIR.Types.Cache(il, out ir);

        public bool Cache<T>(Type il, [NotNullWhen(true)] out T? ir)
            where T : class, IR.IType
            => toIR.Types.Cache(il, out ir);

        public IR.IType Cache(Type il, IR.IType ir)
        {
            if (!toIR.Types.Contains(il))
                toIR.Types.Cache(il, ir);

            if (!toIL.Types.Contains(ir))
                toIL.Types.Cache(ir, il);

            return ir;
        }

        public IR.ICallable? Cache(IL.MethodBase il)
            => toIR.Callables.Cache(il);

        public T? Cache<T>(IL.MethodBase il)
            where T : class, IR.ICallable
            => toIR.Callables.Cache<T>(il);

        public bool Cache(IL.MethodBase il, [NotNullWhen(true)] out IR.ICallable? ir)
            => toIR.Callables.Cache(il, out ir);

        public bool Cache<T>(IL.MethodBase il, [NotNullWhen(true)] out T? ir)
            where T : class, IR.ICallable
            => toIR.Callables.Cache(il, out ir);

        public IR.ICallable Cache(IL.MethodBase il, IR.ICallable ir)
        {
            if (!toIR.Callables.Contains(il))
                toIR.Callables.Cache(il, ir);

            if (!toIL.Callables.Contains(ir))
                toIL.Callables.Cache(ir, il);

            return ir;
        }

        public bool Cache(IL.FieldInfo il, [NotNullWhen(true)] out IR.Field? ir)
            => toIR.Fields.Cache(il, out ir);

        public IR.Field Cache(IL.FieldInfo il, IR.Field ir)
        {
            if (!toIR.Fields.Contains(il))
                toIR.Fields.Cache(il, ir);

            if (!toIL.Fields.Contains(ir))
                toIL.Fields.Cache(ir, il);

            return ir;
        }

        public bool Cache(IL.FieldInfo il, [NotNullWhen(true)] out IR.Global? ir)
            => toIR.Globals.Cache(il, out ir);

        public IR.Global Cache(IL.FieldInfo il, IR.Global ir)
        {
            if (!toIR.Globals.Contains(il))
                toIR.Globals.Cache(il, ir);

            if (!toIL.Globals.Contains(ir))
                toIL.Globals.Cache(ir, il);

            return ir;
        }
    }
}
