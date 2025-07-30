namespace ZSharp.Compiler.ILLoader
{
    partial class ILLoader
    {
        public IType LoadClass(Type @class)
        {
            if (@class.IsGenericTypeDefinition)
                return LoadGenericClass(@class);

            throw new NotImplementedException();
        }
    }
}
