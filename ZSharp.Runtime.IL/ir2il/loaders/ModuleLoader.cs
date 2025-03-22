namespace ZSharp.Runtime.NET.IR2IL
{
    internal sealed class ModuleLoader(IRLoader loader, IR.Module input, IL.Emit.ModuleBuilder output)
        : BaseIRLoader<IR.Module, IL.Emit.ModuleBuilder>(loader, input, output)
    {
        private IL.Emit.TypeBuilder globals = null!;
        private IL.Emit.MethodBuilder cctor = null!;

        public IL.Module Load()
        {
            Context.Cache(Input, Output);

            globals = Output.DefineType(
                Constants.GlobalsTypeName,
                IL.TypeAttributes.Public |
                IL.TypeAttributes.Abstract |
                IL.TypeAttributes.Sealed
            );
            //cctor = globals.DefineMethod(
            //    ".cctor",
            //    IL.MethodAttributes.Static |
            //    IL.MethodAttributes.SpecialName |
            //    IL.MethodAttributes.RTSpecialName |
            //    IL.MethodAttributes.PrivateScope
            //);

            LoadTypes();

            LoadGlobals();

            LoadFunctions();

            Loader.AddToNextPass(() => globals.CreateType());

            return Output;
        }

        private void LoadGlobals()
        {
            if (Input.HasGlobals)
                foreach (var global in Input.Globals)
                    LoadGlobal(global);
        }

        private void LoadFunctions()
        {
            if (Input.HasFunctions)
                foreach (var function in Input.Functions)
                    LoadFunction(function);
        }

        private void LoadTypes()
        {
            if (Input.HasTypes)
                foreach (var type in Input.Types)
                    LoadType(type);
        }

        private Type LoadType(IR.OOPType type)
        {
            if (Context.Cache(type, out var result))
                return result;

            return type switch
            {
                IR.Class @class => LoadClass(@class),
                _ => throw new NotImplementedException()
            };
        }

        private Type LoadClass(IR.Class @class)
        {
            var type = Output.DefineType(@class.Name ?? string.Empty, IL.TypeAttributes.Public);

            Context.Cache(@class, type);

            new ClassLoader(Loader, @class, type).Load();

            type.CreateType();

            return type;
        }

        private IL.FieldInfo LoadGlobal(IR.Global global)
        {
            var result = globals.DefineField(
                global.Name ?? string.Empty,
                Loader.LoadType(global.Type),
                IL.FieldAttributes.Public | IL.FieldAttributes.Static
            );

            Context.Cache(global, result);

            return result;
        }

        private IL.MethodInfo LoadFunction(IR.Function function)
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

            Loader.AddToNextPass(codeLoader.Load);

            return result;
        }
    }
}
