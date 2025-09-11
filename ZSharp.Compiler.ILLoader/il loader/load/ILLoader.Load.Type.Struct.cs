namespace ZSharp.Compiler.ILLoader
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
