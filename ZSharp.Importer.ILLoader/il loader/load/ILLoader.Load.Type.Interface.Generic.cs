namespace ZSharp.Importer.ILLoader
{
    partial class ILLoader
    {
        public CompilerObject LoadGenericInterface(Type type)
        {
            return new Objects.GenericInterface(type, this);
        }
    }
}
