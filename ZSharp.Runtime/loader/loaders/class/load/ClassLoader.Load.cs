namespace ZSharp.Runtime.Loaders
{
    partial class ClassLoader
    {
        protected override void DoLoad()
        {
            if (IRType.HasGenericParameters)
                genericTypeContext = Loader.Runtime.NewTypeContext();

            if (genericTypeContext is not null)
                using (Loader.Runtime.TypeContext(genericTypeContext))
                    LoadAll();
            else
                LoadAll();
        }

        private void LoadAll()
        {
            LoadGenericParameters();

            LoadNestedTypes();

            AddTask(LoadBase);

            AddTask(LoadFields);

            AddTask(LoadConstructors);

            AddTask(LoadMethods);
        }

        private void LoadBase()
        {
            if (IRType.Base is not null)
                ILType.SetParent(Loader.Runtime.ImportType(IRType.Base));
        }

        private void LoadNestedTypes()
        {
            foreach (var type in IRType.NestedTypes)
                LoadNestedType(type);
        }

        private void LoadFields()
        {
            if (IRType.HasFields)
                foreach (var field in IRType.Fields)
                    LoadField(field);
        }

        private void LoadMethods()
        {
            if (IRType.HasMethods)
                foreach (var method in IRType.Methods)
                    LoadMethod(method);
        }

        private void LoadConstructors()
        {
            if (IRType.HasConstructors)
                foreach (var constructor in IRType.Constructors)
                    LoadConstructor(constructor);
        }
    }
}
