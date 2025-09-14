namespace ZSharp.Platform.Runtime.Loaders
{
    partial class InterfaceLoader
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

            AddTask(LoadBases);

            AddTask(LoadMethods);
        }

        private void LoadBases()
        {
            if (IRType.HasBases)
                foreach (var @base in IRType.Bases)
                    ILType.AddInterfaceImplementation(Loader.Runtime.ImportType(@base));
        }

        private void LoadNestedTypes()
        {
            throw new NotSupportedException();
            //foreach (var type in IRType.NestedTypes)
            //    LoadNestedType(type);
        }

        private void LoadMethods()
        {
            if (IRType.HasMethods)
                foreach (var method in IRType.Methods)
                    LoadMethod(method);
        }
    }
}
