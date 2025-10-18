namespace ZSharp.Importer.ILLoader.Objects
{
    public sealed partial class Constructor
        : CompilerObject
    {
        public Constructor(IL.ConstructorInfo il, ILLoader loader)
        {
            IL = il;
        }
    }
}
