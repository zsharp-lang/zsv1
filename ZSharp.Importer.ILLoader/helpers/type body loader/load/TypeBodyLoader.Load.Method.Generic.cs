namespace ZSharp.Importer.ILLoader
{
    partial class TypeBodyLoader
    {
        private CompilerObject LoadGenericMethod(IL.MethodInfo method)
        {
            return new Objects.GenericMethod();
        }
    }
}
