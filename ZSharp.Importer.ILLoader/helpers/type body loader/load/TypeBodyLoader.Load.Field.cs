namespace ZSharp.Importer.ILLoader
{
    partial class TypeBodyLoader
    {
        private CompilerObject LoadField(IL.FieldInfo field)
        {
            return new Objects.Field(field, Loader);
        }
    }
}
