namespace ZSharp.Runtime.Loaders
{
    partial class ClassLoader
    {
        private void LoadField(IR.Field field)
        {
            var attributes = IL.FieldAttributes.Public;

            if (field.IsStatic)
                attributes |= IL.FieldAttributes.Static;

            var il = ILType.DefineField(field.Name, Loader.Runtime.ImportType(field.Type), attributes);

            Loader.Runtime.AddField(field, il);
        }
    }
}
