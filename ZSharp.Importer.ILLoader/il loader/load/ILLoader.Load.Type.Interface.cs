namespace ZSharp.Importer.ILLoader
{
    partial class ILLoader
    {
        public CompilerObject LoadInterface(Type @interface)
        {
            if (@interface.IsGenericTypeDefinition)
                return LoadGenericInterface(@interface);

            return new Objects.Interface(@interface, this);
        }
    }
}
