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
            Context.Cache(Input, Output);

            LoadNestedTypes();

            ModuleLoader.AddToNextPass(LoadDefinition);

            if (Input.HasConstructors)
                ModuleLoader.AddToNextPass(LoadConsructors);

            if (Input.HasFields)
                ModuleLoader.AddToNextPass(LoadFields);

            if (Input.HasMethods)
                ModuleLoader.AddToNextPass(LoadMethods);

            if (Input.InterfacesImplementations.Count > 0)
                ModuleLoader.AddToNextPass(LoadInterfaceImplementations);

            ModuleLoader.AddToCleanUp(() => Output.CreateType());
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
                    // TODO: Context.Uncache(ir);
                }
            });
        }

        private void LoadInterfaceImplementations()
        {
            foreach (var interfaceImplementation in Input.InterfacesImplementations)
            {
                var @interface = Loader.LoadType(interfaceImplementation.Interface);
                Output.AddInterfaceImplementation(@interface);

                foreach (var (@abstract, concrete) in interfaceImplementation.Implementations)
                {
                    var specification = Loader.LoadReference(@abstract);
                    var implementation = Context.Cache<IL.MethodInfo>(concrete) ?? throw new();

                    Output.DefineMethodOverride(implementation, specification);
                }
            }
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
            var result = Output.DefineConstructor(
                IL.MethodAttributes.Public, 
                IL.CallingConventions.HasThis, 
                [.. constructor.Method.Signature.GetParameters().Skip(1).Select(p => Loader.LoadType(p.Type))]
            );

            Context.Cache(constructor.Method, result);

            var context = Code.FunctionCodeContext.From(Loader, result, constructor.Method.UnderlyingFunction);

            foreach (var local in constructor.Method.Body.Locals)
                result.GetILGenerator().DeclareLocal(Loader.LoadType(local.Type));

            var codeLoader = new Code.CodeCompiler(context);

            ModuleLoader.AddToNextPass(() => codeLoader.CompileCode(constructor.Method.Body.Instructions));
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
            var attributes = IL.MethodAttributes.Public;

            if (method.IsStatic)
                attributes |= IL.MethodAttributes.Static;
            if (method.IsVirtual)
                attributes |= IL.MethodAttributes.Virtual | IL.MethodAttributes.NewSlot;

            var result = Output.DefineMethod(
                method.Name ?? Constants.AnonymousMethod,
                attributes,
                Loader.LoadType(method.ReturnType),
                [.. (
                        method.IsInstance || method.IsVirtual 
                        ? method.Signature.GetParameters().Skip(1) 
                        : method.Signature.GetParameters()
                    ).Select(p => Loader.LoadType(p.Type))
                ]
            );

            Context.Cache(method, result);

            var context = Code.FunctionCodeContext.From(Loader, result, method.UnderlyingFunction);

            foreach (var local in method.Body.Locals)
                result.GetILGenerator().DeclareLocal(Loader.LoadType(local.Type));

            var codeLoader = new Code.CodeCompiler(context);

            ModuleLoader.AddToNextPass(() => codeLoader.CompileCode(method.Body.Instructions));
        }

        private static IL.Emit.TypeBuilder CreateDefinition(IL.Emit.ModuleBuilder module, IR.Class @in)
            => module.DefineType(@in.Name ?? Constants.AnonymousClass);

        private static IL.Emit.TypeBuilder CreateDefinition(IL.Emit.TypeBuilder type, IR.Class @in)
            => type.DefineNestedType(@in.Name ?? Constants.AnonymousClass);
    }
}
