namespace ZSharp.Compiler.ILLoader
{
    partial class ModuleBodyLoader
    {
        private CompilerObject LoadGenericMethod(IL.MethodInfo method)
        {
            return new Objects.GenericFunction();
        }
    }
}
