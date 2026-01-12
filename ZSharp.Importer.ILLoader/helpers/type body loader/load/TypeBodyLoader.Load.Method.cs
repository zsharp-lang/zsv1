namespace ZSharp.Importer.ILLoader
{
    partial class TypeBodyLoader
    {
        private CompilerObject LoadMethod(IL.MethodInfo method)
        {
            if (method.IsGenericMethodDefinition)
                return LoadGenericMethod(method);

            return new Objects.Method(method, Loader);
        }
    }
}
