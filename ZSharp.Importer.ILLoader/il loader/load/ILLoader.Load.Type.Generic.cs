namespace ZSharp.Importer.ILLoader
{
    partial class ILLoader
    {
        public CompilerObject LoadConstructedGenericType(Type type)
        {
            return new Objects.GenericTypeInstance(type, this);
        }
    }
}
