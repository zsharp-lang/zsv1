namespace ZSharp.Runtime.Loaders
{
    partial class ClassLoader
    {
        public Type Load()
        {
            if (IRType.HasGenericParameters)
                genericTypeContext = Loader.Runtime.NewTypeContext();

            if (genericTypeContext is not null)
                using (Loader.Runtime.TypeContext(genericTypeContext))
                    LoadAll();
            else
                LoadAll();

            ILType.CreateType();

            return ILType;
        }

        private void LoadAll()
        {
            LoadGenericParameters();

            LoadNestedTypes();

            AddTask(LoadFields);

            AddTask(LoadConstructors);

            AddTask(LoadMethods);
        }

        private void LoadNestedTypes()
        {
            throw new NotImplementedException();
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
