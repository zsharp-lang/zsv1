namespace ZSharp.Importer.ILLoader
{
    partial class ModuleBodyLoader
    {
        private CompilerObject LoadGenericMethod(IL.MethodInfo method)
        {
            return new Objects.GenericFunction();
        }
    }
}
