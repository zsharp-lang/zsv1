namespace ZSharp.Platform.Runtime.Loaders
{
    partial class EnumClassLoader
    {
        protected override void DoLoad()
        {
            LoadAll();
        }

        private void LoadAll()
        {
            CreateValueField();

            LoadNestedTypes();

            AddTask(LoadValues);

            AddTask(LoadMethods);
        }

        private void CreateValueField()
        {
            ILType.DefineField(
                "__value",
                Loader.Runtime.ImportType(IRType.Type),
                IL.FieldAttributes.Public | IL.FieldAttributes.SpecialName | IL.FieldAttributes.RTSpecialName
            );
        }

        private void LoadNestedTypes()
        {
            //foreach (var type in IRType.NestedTypes)
            //    LoadNestedType(type);
        }

        private void LoadValues()
        {
            foreach (var value in IRType.Values)
                LoadValue(value);
        }

        private void LoadMethods()
        {
            //if (IRType.HasMethods)
            //    foreach (var method in IRType.Methods)
            //        LoadMethod(method);
        }
    }
}
