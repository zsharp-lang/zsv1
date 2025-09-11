namespace ZSharp.Compiler.ILLoader
{
    partial class ILLoader
    {
        public CompilerObject LoadInterface(Type @interface)
        {
            if (@interface.IsGenericTypeDefinition)
                return LoadGenericInterface(@interface);

            throw new NotImplementedException();
        }
    }
}
