namespace ZSharp.Importer.ILLoader.Objects
{
    public sealed partial class Method
        : CompilerObject
    {
        public Method(IL.MethodInfo il, ILLoader loader)
        {
            IL = il;
            Loader = loader;

            if (!IL.IsStatic)
                signature.parameters.Add(new ThisParameter(IL, loader));

            foreach (var parameter in IL.GetParameters())
                signature.parameters.Add(new Parameter(parameter, loader));
        }
    }
}
