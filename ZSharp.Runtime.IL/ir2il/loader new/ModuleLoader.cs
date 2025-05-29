using System;

namespace ZSharp.Runtime.NET.IR2IL
{
    internal sealed class ModuleLoader(
        RootModuleLoader loader, 
        IR.Module @in,
        IL.Emit.ModuleBuilder? @out = null
    ) : ModuleContentLoader<IR.Module, IL.Emit.ModuleBuilder>(
        loader, 
        @in, 
        @out ?? loader.AssemblyBuilder.DefineDynamicModule(@in.Name ?? Constants.AnonymousModule)
        )
    {
        protected override void DoLoad()
        {
            if (Input.HasSubmodules)
                LoadSubModules();

            if (Input.HasTypes)
                LoadTypes();

            if (!Input.HasFunctions && !Input.HasGlobals)
                return;

            var globals = Output.DefineType(Constants.GlobalsTypeName, IL.TypeAttributes.Public | IL.TypeAttributes.Abstract | IL.TypeAttributes.Sealed);

            if (Input.HasFunctions)
                ModuleLoader.AddToNextPass(() => LoadFunctions(globals));

            if (Input.HasGlobals)
                ModuleLoader.AddToNextPass(() => LoadGlobals(globals));

            ModuleLoader.AddToCleanUp(() =>
            {
                globals.CreateType();
            });
        }

        private void LoadSubModules()
        {
            foreach (var submodule in Input.Submodules)
                new ModuleLoader(ModuleLoader, submodule).Load();
        }

        private void LoadTypes()
        {
            foreach (var type in Input.Types)
                (type switch
                {
                    IR.Class @class => new ClassLoader(ModuleLoader, Output, @class).Load,
                    IR.Interface @interface => new InterfaceLoader(loader, Output, @interface).Load,
                    _ => (Func<IL.Emit.TypeBuilder>)null! ?? throw new NotImplementedException()
                })();
        }

        private void LoadFunctions(IL.Emit.TypeBuilder globals)
        {
            foreach (var function in Input.Functions)
                LoadFunction(globals, function);
        }

        private void LoadGlobals(IL.Emit.TypeBuilder globals)
        {
            foreach (var global in Input.Globals)
                LoadGlobal(globals, global);
        }

        private IL.MethodInfo LoadFunction(IL.Emit.TypeBuilder globals, IR.Function function)
        {
            var irParams = function.Signature.GetParameters();

            var attributes = IL.MethodAttributes.Public | IL.MethodAttributes.Static;

            var parameters = irParams.Select(p => new Parameter()
            {
                Name = p.Name,
                Type = Loader.LoadType(p.Type),
                Position = p.Index,
            });

            var result = globals.DefineMethod(
                function.Name ?? string.Empty,
                attributes,
                Loader.LoadType(function.ReturnType),
                parameters.Select(p => p.Type).ToArray()
            );

            Context.Cache(function, result);

            var ilGen = result.GetILGenerator();
            var codeLoader = new CodeLoader(Loader, function, ilGen);

            foreach (var (ir, parameter) in irParams.Zip(parameters))
                codeLoader.Args[ir] = parameter;

            foreach (var local in function.Body.Locals)
                codeLoader.Locals[local] = ilGen.DeclareLocal(Loader.LoadType(local.Type));

            ModuleLoader.AddToNextPass(() => codeLoader.Load());

            return result;
        }

        private IL.FieldInfo LoadGlobal(IL.Emit.TypeBuilder globals, IR.Global global)
        {
            var result = globals.DefineField(
                global.Name ?? string.Empty,
                Loader.LoadType(global.Type),
                IL.FieldAttributes.Public | IL.FieldAttributes.Static
            );

            Context.Cache(global, result);

            return result;
        }
    }
}
