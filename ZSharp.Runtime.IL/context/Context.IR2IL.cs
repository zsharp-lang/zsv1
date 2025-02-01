using System.Diagnostics.CodeAnalysis;

namespace ZSharp.Runtime.NET
{
    partial class Context
    {
        public IL.Module? Cache(IR.Module ir)
            => toIL.Modules.Cache(ir);

        public bool Cache(IR.Module ir, [NotNullWhen(true)] out IL.Module? il)
            => toIL.Modules.Cache(ir, out il);

        public IL.Module Cache(IR.Module ir, IL.Module il)
        {
            if (!toIR.Modules.Contains(il))
                toIR.Modules.Cache(il, ir);

            if (!toIL.Modules.Contains(ir))
                toIL.Modules.Cache(ir, il);

            return il;
        }

        public Type? Cache(IR.IType ir)
            => toIL.Types.Cache(ir);

        public bool Cache(IR.IType ir, [NotNullWhen(true)] out Type? il)
            => toIL.Types.Cache(ir, out il);

        public Type Cache(IR.IType ir, Type il)
        {
            if (!toIR.Types.Contains(il))
                toIR.Types.Cache(il, ir);

            if (!toIL.Types.Contains(ir))
                toIL.Types.Cache(ir, il);

            return il;
        }

        public Type? Cache(IR.OOPType ir)
            => toIL.OOPTypes.Cache(ir);

        public bool Cache(IR.OOPType ir, [NotNullWhen(true)] out Type? il)
            => toIL.OOPTypes.Cache(ir, out il);

        public Type Cache(IR.OOPType ir, Type il)
        {
            if (!toIR.OOPTypes.Contains(il))
                toIR.OOPTypes.Cache(il, ir);

            if (!toIL.OOPTypes.Contains(ir))
                toIL.OOPTypes.Cache(ir, il);

            return il;
        }

        public IL.MethodBase? Cache(IR.ICallable ir)
            => toIL.Callables.Cache(ir);

        public T? Cache<T>(IR.ICallable ir)
            where T : IL.MethodBase
            => toIL.Callables.Cache<T>(ir);

        public bool Cache(IR.ICallable ir, [NotNullWhen(true)] out IL.MethodBase? il)
            => toIL.Callables.Cache(ir, out il);

        public bool Cache<T>(IR.ICallable ir, [NotNullWhen(true)] out T? il)
            where T : IL.MethodBase
            => toIL.Callables.Cache(ir, out il);

        public IL.MethodBase Cache(IR.ICallable ir, IL.MethodBase il)
        {
            if (!toIR.Callables.Contains(il))
                toIR.Callables.Cache(il, ir);

            if (!toIL.Callables.Contains(ir))
                toIL.Callables.Cache(ir, il);

            return il;
        }

        public IL.FieldInfo? Cache(IR.Field ir)
            => toIL.Fields.Cache(ir);

        public bool Cache(IR.Field ir, [NotNullWhen(true)] out IL.FieldInfo? il)
            => toIL.Fields.Cache(ir, out il);

        public IL.FieldInfo Cache(IR.Field ir, IL.FieldInfo il)
        {
            if (!toIR.Fields.Contains(il))
                toIR.Fields.Cache(il, ir);

            if (!toIL.Fields.Contains(ir))
                toIL.Fields.Cache(ir, il);

            return il;
        }

        public IL.FieldInfo? Cache(IR.Global ir)
            => toIL.Globals.Cache(ir);

        public bool Cache(IR.Global ir, [NotNullWhen(true)] out IL.FieldInfo? il)
            => toIL.Globals.Cache(ir, out il);

        public IL.FieldInfo Cache(IR.Global ir, IL.FieldInfo il)
        {
            if (!toIR.Globals.Contains(il))
                toIR.Globals.Cache(il, ir);

            if (!toIL.Globals.Contains(ir))
                toIL.Globals.Cache(ir, il);

            return il;
        }
    }
}
