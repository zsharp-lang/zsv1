namespace ZSharp.Runtime.Loaders
{
    internal static class TypeLoaderHelper
    {
        public static Type LoadType(EmitLoader loader, Emit.TypeBuilder parentBuilder, IR.OOPType type)
        {
            var tasks = new TaskManager(() => { });
            var result = type switch
            {
                IR.Class def => new ClassLoader(loader)
                {
                    ILType = parentBuilder.DefineNestedType(
                        def.Name ?? throw new(),
                        IL.TypeAttributes.Public
                    ),
                    IRType = def,
                    Tasks = tasks
                }.Load(),
                IR.Interface def => new InterfaceLoader(loader)
                {
                    ILType = parentBuilder.DefineNestedType(
                        def.Name ?? throw new(),
                        IL.TypeAttributes.Public | IL.TypeAttributes.Interface
                    ),
                    IRType = def,
                    Tasks = tasks
                }.Load(),
                _ => throw new NotSupportedException(),
            };
            loader.Runtime.AddTypeDefinition(type, result);
            tasks.RunUntilComplete();
            return result;
        }
    }
}
