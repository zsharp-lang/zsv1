namespace ZSharp.Runtime.Loaders
{
    partial class ModuleLoader
    {
        public void LoadClass(IR.Class @class)
        {
            ClassLoader loader = new(Loader)
            {
                ILType = ILModule.DefineType(@class.Name ?? string.Empty, IL.TypeAttributes.Public),
                IRType = @class,
                Tasks = tasks
            };

            Loader.Runtime.AddTypeDefinition(@class, loader.ILType);

            Loader.Runtime.SetTypeDefinition(@class, loader.Load());
        }
    }
}
