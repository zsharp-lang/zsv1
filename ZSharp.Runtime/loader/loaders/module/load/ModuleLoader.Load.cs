namespace ZSharp.Runtime.Loaders
{
    partial class ModuleLoader
    {
        public IL.Module Load()
        {
            RunUntilComplete();

            Globals.CreateType();

            return ILModule;
        }

        private void LoadAll()
        {
            LoadSubmodules();

            LoadTypes();

            LoadGlobals();

            LoadFunctions();

            LoadInitializer();
        }

        private void LoadSubmodules()
        {
            if (IRModule.HasSubmodules)
                foreach (var module in IRModule.Submodules)
                    Loader.Runtime.AddModule(
                        module,
                        new ModuleLoader(Loader, module)
                        {
                            AssemblyBuilder = AssemblyBuilder
                        }.Load()
                    );
        }

        private void LoadTypes()
        {
            if (IRModule.HasTypes)
                foreach (var type in IRModule.Types)
                    LoadType(type);
        }

        private void LoadGlobals()
        {
            if (IRModule.HasGlobals)
                foreach (var global in IRModule.Globals)
                    LoadGlobal(global);
        }

        private void LoadFunctions()
        {
            if (IRModule.HasFunctions)
                foreach (var function in IRModule.Functions)
                    LoadFunction(function);
        }

        private void LoadInitializer()
        {
            if (IRModule.Initializer is null) return;

            var typeInitializer = Globals.DefineTypeInitializer();

            var initializationFunction = Loader.Runtime.ImportFunction(IRModule.Initializer);

            if (!initializationFunction.IsStatic)
                throw new InvalidProgramException(
                    $"Module's {ILModule.Name} initializer function must be static"
                );
            if (initializationFunction.ReturnType != typeof(void))
                throw new InvalidProgramException(
                    $"Module's {ILModule.Name} initializer function must return void"
                );
            if (initializationFunction.GetParameters().Length != 0)
                throw new InvalidProgramException(
                    $"Module's {ILModule.Name} initializer function must not have any parameters"
                );

            var il = typeInitializer.GetILGenerator();

            il.Emit(Emit.OpCodes.Call, initializationFunction);
            il.Emit(Emit.OpCodes.Ret);
        }
    }
}
