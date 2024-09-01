using ZSharp.IR;

namespace ZSharp.VM
{
    public sealed partial class Runtime
    {
        private readonly Assembler assembler = new(runtimeModule);

        public ZSObject LoadIRInternal(IType type)
        {
            if (type is IRObject @object)
                return LoadIR(@object);
            throw new NotImplementedException();
        }

        public ZSFunction LoadIRInternal(Function function)
        {
            if (!function.HasBody)
                throw new Exception();

            var code = assembler.Assemble(function.Body.Instructions, function);

            return new(code.Instructions)
            {
                LocalCount = function.Body.HasLocals ? function.Body.Locals.Count : 0,
                StackSize = code.StackSize,
            };
        }

        public ZSModule LoadIRInternal(Module module)
        {
            var zsModule = new ZSModule(module);

            foreach (var submodule in module.Submodules)
                LoadIR(submodule);

            foreach (var function in module.Functions)
                LoadIR(function);

            return zsModule;
        }
    }
}
