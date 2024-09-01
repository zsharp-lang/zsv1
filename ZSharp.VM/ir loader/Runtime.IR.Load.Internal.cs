using ZSharp.IR;

namespace ZSharp.VM
{
    public sealed partial class Runtime
    {
        private readonly Assembler assembler;

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

            //var code = assembler.Assemble(function.Body.Instructions, function);

            return new([])
            {
                ArgumentCount = function.Signature.Length,
                LocalCount = function.Body.HasLocals ? function.Body.Locals.Count : 0,
                StackSize = 0,
            };
        }

        public ZSModule LoadIRInternal(Module module)
        {
            var zsModule = new ZSModule(module);

            if (module.HasImportedModules)
                foreach (var importedModule in module.ImportedModules)
                    LoadIR(importedModule);

            foreach (var function in module.Functions)
                LoadIR(function);

            foreach (var submodule in module.Submodules)
                LoadIR(submodule);

            foreach (var function in module.Functions)
            {
                var zsFunction = LoadIR(function);
                var functionBody = assembler.Assemble(function.Body.Instructions, function);
                
                zsFunction.Code = functionBody.Instructions;
                zsFunction.StackSize = functionBody.StackSize;
            }

            return zsModule;
        }
    }
}
