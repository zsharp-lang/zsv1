using CommonZ.Utils;

namespace ZSharp.VM
{
    public sealed partial class Runtime
    {
        private readonly Mapping<IR.Module, InternalModule> internalModules = [];
        private readonly Mapping<IR.Function, ZSInternalFunction> internalFunctions = [];

        public IR.Module AddInternalModule(InternalModule module)
        {
            var ir = module.Load(this);

            if (internalModules.ContainsKey(ir))
                throw new InvalidOperationException("Internal module already exists.");

            internalModules[ir] = module;

            // TODO: Load the module using a custom loader.
            // Or just call the initializer. (althoug it's already called at the top?)
            irMap.Cache(ir, ZSModule.CreateFrom(ir, TypeSystem.Module));

            foreach (var function in ir.Functions)
                irMap.Cache(function, GetInternalFunction(function));

            return ir;
        }

        public ZSInternalFunction GetInternalFunction(IR.Function function)
        {
            if (internalFunctions.TryGetValue(function, out var internalFunction))
                return internalFunction;

            if (function.Module is null) 
                throw new InvalidOperationException("Internal function must belong to a module.");

            if (function.HasBody)
                throw new InvalidOperationException("Internal function must not have a body.");

            if (!internalModules.TryGetValue(function.Module, out var internalModule))
                throw new InvalidOperationException(
                    "Function belongs to a function that is either not loaded" +
                    " or not an internal module."
                );

            if (!internalModule.FunctionImplementations.TryGetValue(function, out var implementation))
                throw new InvalidOperationException("Internal function not implemented.");

            return internalFunctions[function] = new(
                implementation, 
                function.Signature.Length, 
                TypeSystem.Function
            );
        }
    }
}
