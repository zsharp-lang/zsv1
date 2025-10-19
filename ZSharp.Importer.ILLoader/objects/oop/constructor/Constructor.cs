namespace ZSharp.Importer.ILLoader.Objects
{
    internal sealed partial class Constructor
        : CompilerObject
    {
        public Constructor(IL.ConstructorInfo il, ILLoader loader)
        {
            IL = il;
            Loader = loader;

            thisParameter = new(il, loader);

            signature.parameters.Add(thisParameter);

            foreach (var parameter in il.GetParameters())
                signature.parameters.Add(new Parameter(parameter, loader));
        }
    }
}
