namespace ZSharp.Runtime.NET.IR2IL
{
    internal sealed class ClassLoader(IRLoader loader, IR.Class input, IL.Emit.TypeBuilder output)
        : BaseIRLoader<IR.Class, IL.Emit.TypeBuilder>(loader, input, output)
    {
        private List<Action> thisPass = [], nextPass = [];

        private void AddToNextPass(Action action)
            => nextPass.Add(action);

        public void Load()
        {
            //LoadNestedTypes();

            LoadBase();

            //LoadInterfaces();

            LoadFields();

            LoadConstructors();

            //LoadEvents();

            LoadMethods();

            //LoadProperties();

            do
            {
                (thisPass, nextPass) = (nextPass, thisPass);
                nextPass.Clear();

                foreach (var action in thisPass)
                    action();
            } while (thisPass.Count > 0);
        }

        private void LoadNestedTypes()
        {
            throw new NotImplementedException();
        }

        private void LoadBase()
        {
            if (Input.Base != null)
                Output.SetParent(Loader.LoadType(Input.Base));
        }

        private void LoadInterfaces()
        {
            throw new NotImplementedException();
        }

        private void LoadConstructors()
        {
            foreach (var constructor in Input.Constructors)
                LoadConstructor(constructor);
        }

        private void LoadFields()
        {
            foreach (var field in Input.Fields)
                LoadField(field);
        }

        private void LoadEvents()
        {
            throw new NotImplementedException();
        }

        private void LoadMethods()
        {
            foreach (var method in Input.Methods)
                LoadMethod(method);
        }

        private void LoadProperties()
        {
            throw new NotImplementedException();
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

            var result = Output.DefineConstructor(IL.MethodAttributes.Public, IL.CallingConventions.HasThis, irParams.Skip(1).Select(p => Loader.LoadType(p.Type)).ToArray());

            Context.Cache(constructor.Method, result);

            var ilGen = result.GetILGenerator();
            var codeLoader = new CodeLoader(Loader, constructor.Method, ilGen);

            foreach (var (ir, parameter) in irParams.Zip(parameters))
                codeLoader.Args[ir] = parameter;

            foreach (var local in constructor.Method.Body.Locals)
                codeLoader.Locals[local] = ilGen.DeclareLocal(Loader.LoadType(local.Type));

            codeLoader.Load();
        }

        private void LoadField(IR.Field field)
        {
            var il = Output.DefineField(field.Name, Loader.LoadType(field.Type), IL.FieldAttributes.Public);
            
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
                (method.IsInstance || method.IsVirtual ? parameters.Skip(1) : parameters).Select(p => p.Type).ToArray()
            );

            Context.Cache(method, result);

            var ilGen = result.GetILGenerator();
            var codeLoader = new CodeLoader(Loader, method, ilGen);

            foreach (var (ir, parameter) in irParams.Zip(parameters))
                codeLoader.Args[ir] = parameter;

            foreach (var local in method.Body.Locals)
                codeLoader.Locals[local] = ilGen.DeclareLocal(Loader.LoadType(local.Type));

            AddToNextPass(() => codeLoader.Load());
        }
    }
}
