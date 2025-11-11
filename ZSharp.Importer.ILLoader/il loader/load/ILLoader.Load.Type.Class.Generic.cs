namespace ZSharp.Importer.ILLoader
{
    partial class ILLoader
    {
        public CompilerObject LoadGenericClass(Type @class)
        {
            return new Objects.GenericClass(@class, this);
        }
    }
}
