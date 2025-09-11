namespace ZSharp.Compiler.ILLoader.Objects
{
    public sealed partial class Method
        : CompilerObject
    {
        public Method(IL.MethodInfo il, ILLoader loader)
        {
            IL = il;
            Loader = loader;
        }
    }
}
