namespace ZSharp.Importer.ILLoader
{
    partial class ModuleBodyLoader
    {
        private CompilerObject LoadProperty(IL.PropertyInfo property)
        {
            return new Objects.GlobalProperty();
        }
    }
}
