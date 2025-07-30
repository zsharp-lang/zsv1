namespace ZSharp.Compiler.ILLoader
{
    partial class ILLoader
    {
        public IType LoadInterface(Type @interface)
        {
            if (@interface.IsGenericTypeDefinition)
                return LoadGenericInterface(@interface);

            throw new NotImplementedException();
        }
    }
}
