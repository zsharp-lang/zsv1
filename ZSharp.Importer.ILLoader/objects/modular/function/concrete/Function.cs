namespace ZSharp.Importer.ILLoader.Objects
{
    public sealed partial class Function
        : CompilerObject
    {
        public Function(IL.MethodInfo il, ILLoader loader)
        {
            IL = il;
            Loader = loader;

            ReturnType = loader.LoadType(IL.ReturnType);

            foreach (var parameter in IL.GetParameters())
                signature.parameters.Add(new Parameter(parameter, loader));
        }
    }
}
