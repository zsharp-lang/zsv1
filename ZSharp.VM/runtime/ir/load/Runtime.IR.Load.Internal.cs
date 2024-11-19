﻿using ZSharp.IR;

namespace ZSharp.VM
{
    public sealed partial class Runtime
    {
        private ZSObject LoadIRInternal(IType type)
        {
            if (type is IRObject @object)
                return Get(@object);
            throw new NotImplementedException();
        }

        private ZSClass LoadIRInternal(Class @class)
        {
            ZSClass result = ZSClass.CreateFrom(@class, TypeSystem.Type);

            foreach (var method in @class.Methods)
                LoadIRInternal(method);

            return irMap.Cache(@class, result);
        }

        private ZSFunction LoadIRInternal(Function function)
        {
            if (!function.HasBody)
                throw new Exception();

            var code = 
                function.Module is not null 
                ? [] 
                : Assemble(function.Body.Instructions, function).Instructions;

            return irMap.Cache(function, ZSFunction.CreateFrom(
                function,
                code, 
                TypeSystem.Function
            ));
        }

        private ZSMethod LoadIRInternal(Method method)
        {
            if (!method.HasBody)
                throw new Exception();

            //var code =
            //    method.Module is not null
            //    ? []
            //    : Assemble(method.Body.Instructions, method.Function).Instructions;

            return irMap.Cache(method, ZSMethod.CreateFrom(
                method,
                LoadIRInternal(method.Function),
                TypeSystem.Function
            ));
        }

        private ZSModule LoadIRInternal(Module module)
        {
            var zsModule = ZSModule.CreateFrom(module, TypeSystem.Module);

            if (module.HasImportedModules)
                foreach (var importedModule in module.ImportedModules)
                    ImportIR(importedModule.Imported);

            foreach (var function in module.Functions)
                LoadIRInternal(function);

            foreach (var type in module.Types)
                _ = type switch
                {
                    Class @class => LoadIRInternal(@class),
                    _ => throw new NotImplementedException(),
                };

            foreach (var submodule in module.Submodules)
                LoadIRInternal(submodule);

            irMap.Cache(module, zsModule);

            foreach (var function in module.Functions)
            {
                var zsFunction = Get(function);
                var functionBody = Assemble(function.Body.Instructions, function);
                
                zsFunction.Code = functionBody.Instructions;
                zsFunction.StackSize = functionBody.StackSize;
            }

            if (module.Functions.Count > 0 && module.Functions[0].Name is null)
            {
                var initFunction = Get(module.Functions[0]);

                Run(new(initFunction.Code, initFunction.StackSize), new ZSObject[initFunction.LocalCount]);
            }

            return zsModule;
        }
    }
}
