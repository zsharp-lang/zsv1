namespace ZSharp.Importer.ILLoader
{
    partial class ILLoader
    {
        public CompilerObject LoadStruct(Type @struct)
        {
            if (@struct.IsGenericTypeDefinition)
                return LoadGenericStruct(@struct);

            throw new NotImplementedException();
        }
    }
}
