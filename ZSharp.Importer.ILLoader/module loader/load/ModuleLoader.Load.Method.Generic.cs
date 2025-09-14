namespace ZSharp.Importer.ILLoader
{
    partial class ModuleLoader
    {
        private CompilerObject LoadGenericMethod(IL.MethodInfo method)
        {
            return new Objects.GenericFunction();
        }
    }
}
