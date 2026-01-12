namespace ZSharp.Importer.ILLoader
{
    partial class ModuleBodyLoader
    {
        private CompilerObject LoadField(IL.FieldInfo field)
        {
            if (!field.IsStatic) throw new ArgumentException("Only static fields are supported.", nameof(field));

            return new Objects.Global(field, Loader);
        }
    }
}
