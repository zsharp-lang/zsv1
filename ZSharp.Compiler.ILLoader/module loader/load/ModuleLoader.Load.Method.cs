namespace ZSharp.Compiler.ILLoader
{
    partial class ModuleLoader
    {
        private CompilerObject LoadMethod(IL.MethodInfo method)
        {
            if (!method.IsStatic) throw new ArgumentException("Only static methods are supported.", nameof(method));

            if (method.IsGenericMethodDefinition)
                return LoadGenericMethod(method);

            return new Objects.Function(method, Loader);
        }
    }
}
