namespace ZSharp.Compiler.ILLoader
{
    partial class ILLoader
    {
        public CompilerObject LoadMethod(IL.MethodInfo method)
        {
            if (method.IsGenericMethodDefinition)
                return LoadGenericMethod(method);

            throw new NotImplementedException();
        }
    }
}
