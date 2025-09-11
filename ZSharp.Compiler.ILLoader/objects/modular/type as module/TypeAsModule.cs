using System.Reflection;

namespace ZSharp.Compiler.ILLoader.Objects
{
    internal sealed partial class TypeAsModule
        : CompilerObject
    {
        public string Name => IL.Name;

        public TypeAsModule(Type il, ILLoader loader)
        {
            IL = il;
            Loader = loader;
            BodyLoader = new(this, loader);

            if (il.GetCustomAttribute<SetNamespaceAttribute>() is SetNamespaceAttribute setNamespace)
                RootNamespace = setNamespace.Name;
            else RootNamespace = string.Empty;

            Prepare();
        }
    }
}
