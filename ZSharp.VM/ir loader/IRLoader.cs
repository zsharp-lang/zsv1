using ZSharp.IR;

namespace ZSharp.VM
{
    internal sealed class IRLoader
    {
        private readonly Assembler assembler = new();
        private readonly Dictionary<IRObject, ZSObject> irMap = [];

        public Assembler Assembler => assembler;

        public ZSObject Load(IRObject ir)
        {
            if (irMap.TryGetValue(ir, out var zs))
                return zs;

            return irMap[ir] = ir switch
            {
                Function function => Load(function),
                Module module => Load(module),
                Signature signature => Load(signature),
                _ => throw new NotImplementedException(),
            };
        }

        public ZSObject Load(IR.IType type)
        {
            if (type is IRObject @object)
                return Load(@object);
            throw new NotImplementedException();
        }

        public ZSObject Reload(IRObject ir)
        {
            Unload(ir);
            return Load(ir);
        }

        public void Unload(IRObject ir)
        {
            irMap.Remove(ir);
        }

        public void Set(IRObject ir, ZSObject value)
        {
            irMap[ir] = value;
        }

        private ZSFunction Load(Function function)
        {
            if (!function.HasBody)
                throw new Exception();

            var signature = Load(function.Signature);
            var code = assembler.Assemble(function.Body.Instructions);

            return new(signature, code)
            {
                LocalCount = function.Body.HasLocals ? function.Body.Locals.Count : 0,
                StackSize = function.Body.StackSize,
            };
        }

        private ZSModule Load(Module module)
        {
            var zsModule = new ZSModule(module);

            //foreach (var global in module.Globals)
            //    zsModule.SetGlobal(global, Load(global.Initializer));

            return zsModule;
        }

        private ZSSignature Load(Signature signature)
        {
            ZSSignature result = new(signature);

            if (signature.HasArgs)
                foreach (var arg in signature.Args.Parameters)
                    result.Args.Add(Load(arg.Type));
            if (signature.IsVarArgs)
                result.VarArgs = Load(signature.Args.Var!.Type);
            if (signature.HasKwArgs)
                foreach (var (name, arg) in signature.KwArgs.Parameters)
                    result.KwArgs[name] = Load(arg.Type);
            if (signature.IsVarKwArgs)
                result.VarKwArgs = Load(signature.KwArgs.Var!.Type);

            return result;
        }
    }
}
