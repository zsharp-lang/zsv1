namespace ZSharp.Compiler.ILLoader
{
    partial class ILLoader
    {
        public IType LoadStruct(Type @struct)
        {
            if (@struct.IsGenericTypeDefinition)
                return LoadGenericStruct(@struct);

            throw new NotImplementedException();
        }
    }
}
