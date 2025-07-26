namespace ZSharp.Compiler.ILLoader.Objects
{
    public sealed partial class Module
        : CompilerObject
    {
        public string Name => IL.Name;

        public Module(IL.Module il, ILLoader loader)
        {
            IL = il;
            Globals = IL.GetType("<Module>");
            Loader = loader;

            foreach (var type in il.GetTypes())
                if (!type.IsPublic) continue;
                else if (type.Namespace is not null)
                    Loader.Namespace(type.Namespace).AddLazyMember(type.Name, () => Loader.LoadType(type));
        }
    }
}
