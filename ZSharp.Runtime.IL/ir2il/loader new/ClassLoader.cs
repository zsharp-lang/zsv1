namespace ZSharp.Runtime.NET.IR2IL
{
    internal sealed class ClassLoader(RootModuleLoader loader, IR.Class @in, IL.Emit.TypeBuilder @out)
        : ModuleContentLoader<IR.Class, IL.Emit.TypeBuilder>(loader, @in, @out)
    {
        public ClassLoader(RootModuleLoader loader, IL.Emit.ModuleBuilder module, IR.Class @in)
            : this(loader, @in, CreateDefinition(module, @in)) { }

        public ClassLoader(RootModuleLoader loader, IL.Emit.TypeBuilder type, IR.Class @in)
            : this(loader, @in, CreateDefinition(type, @in)) { }

        protected override void DoLoad()
        {
            LoadNestedTypes();

            ModuleLoader.AddToNextPass(LoadDefinition);

            if (Input.HasConstructors)
                ModuleLoader.AddToNextPass(LoadConsructors);

            if (Input.HasFields)
                ModuleLoader.AddToNextPass(LoadFields);

            if (Input.HasMethods)
                ModuleLoader.AddToNextPass(LoadMethods);
        }

        private void LoadDefinition()
        {
            if (Input.Base is not null)
                Output.SetParent(Loader.LoadType(Input.Base));

            if (Input.HasGenericParameters)
                LoadGenericParameters();
        }

        private void LoadGenericParameters()
        {
            var genericParameters = Output.DefineGenericParameters(
                Input.GenericParameters.Select(gp => gp.Name).ToArray()
            );

            foreach (var (ir, il) in Input.GenericParameters.Zip(genericParameters))
                Context.Cache(ir, il);

            ModuleLoader.AddToCleanUp(() =>
            {
                foreach (var ir in Input.GenericParameters)
                {
                    //Context.Uncache(ir);
                }
            });
        }

        private void LoadNestedTypes()
        {
            
        }

        private void LoadConsructors()
        {
            foreach (var constructor in Input.Constructors)
                LoadConstructor(constructor);
        }

        private void LoadFields()
        {
            foreach (var field in Input.Fields)
                LoadField(field);
        }

        private void LoadMethods()
        {
            foreach (var method in Input.Methods)
                LoadMethod(method);
        }

        private void LoadConstructor(IR.Constructor constructor)
        {
            var irParams = constructor.Method.Signature.GetParameters();

            var parameters = irParams.Select(p => new Parameter()
            {
                Name = p.Name,
                Type = Loader.LoadType(p.Type),
                Position = p.Index
            });

            var result = Output.DefineConstructor(IL.MethodAttributes.Public, IL.CallingConventions.HasThis, [.. irParams.Skip(1).Select(p => Loader.LoadType(p.Type))]);

            Context.Cache(constructor.Method, result);

            var ilGen = result.GetILGenerator();
            var codeLoader = new CodeLoader(ModuleLoader, constructor.Method, ilGen);

            foreach (var (ir, parameter) in irParams.Zip(parameters))
                codeLoader.Args[ir] = parameter;

            foreach (var local in constructor.Method.Body.Locals)
                codeLoader.Locals[local] = ilGen.DeclareLocal(Loader.LoadType(local.Type));

            ModuleLoader.AddToNextPass(() => codeLoader.Load());
        }

        private void LoadField(IR.Field field)
        {
            var attributes = IL.FieldAttributes.Public;

            if (field.IsStatic)
                attributes |= IL.FieldAttributes.Static;

            var il = Output.DefineField(field.Name, Loader.LoadType(field.Type), attributes);

            Context.Cache(field, il);
        }

        private void LoadMethod(IR.Method method)
        {
            var irParams = method.Signature.GetParameters();

            var parameters = irParams.Select(p => new Parameter()
            {
                Name = p.Name,
                Type = Loader.LoadType(p.Type),
                Position = p.Index,
            });

            var attributes = IL.MethodAttributes.Public;

            if (method.IsStatic)
                attributes |= IL.MethodAttributes.Static;

            var result = Output.DefineMethod(
                method.Name ?? Constants.AnonymousMethod,
                attributes,
                Loader.LoadType(method.ReturnType),
                [.. (method.IsInstance || method.IsVirtual ? parameters.Skip(1) : parameters).Select(p => p.Type)]
            );

            Context.Cache(method, result);

            var ilGen = result.GetILGenerator();
            var codeLoader = new CodeLoader(ModuleLoader, method, ilGen);

            foreach (var (ir, parameter) in irParams.Zip(parameters))
                codeLoader.Args[ir] = parameter;

            foreach (var local in method.Body.Locals)
                codeLoader.Locals[local] = ilGen.DeclareLocal(Loader.LoadType(local.Type));

            ModuleLoader.AddToNextPass(() => codeLoader.Load());
        }

        private static IL.Emit.TypeBuilder CreateDefinition(IL.Emit.ModuleBuilder module, IR.Class @in)
            => module.DefineType(@in.Name ?? Constants.AnonymousClass);

        private static IL.Emit.TypeBuilder CreateDefinition(IL.Emit.TypeBuilder type, IR.Class @in)
            => type.DefineNestedType(@in.Name ?? Constants.AnonymousClass);
    }
}
