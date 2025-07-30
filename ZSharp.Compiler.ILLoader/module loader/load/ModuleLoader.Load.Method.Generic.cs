namespace ZSharp.Compiler.ILLoader
{
    partial class ModuleLoader
    {
        private CompilerObject LoadGenericMethod(IL.MethodInfo method)
        {
            return new Objects.GenericFunction();
        }
    }
}
