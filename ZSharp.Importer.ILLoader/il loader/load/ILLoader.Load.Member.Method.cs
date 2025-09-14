namespace ZSharp.Importer.ILLoader
{
    partial class ILLoader
    {
        public CompilerObject LoadMethod(IL.MethodInfo method)
        {
            if (method.IsGenericMethodDefinition)
                return LoadGenericMethod(method);

            var result = new Objects.Method(method, this);

            return result;
        }
    }
}
