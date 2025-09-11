namespace ZSharp.Compiler.ILLoader
{
    partial class ILLoader
    {
        public CompilerObject LoadClass(Type @class)
        {
            if (@class.IsGenericTypeDefinition)
                return LoadGenericClass(@class);

            return new Objects.Class(@class, this);
        }
    }
}
