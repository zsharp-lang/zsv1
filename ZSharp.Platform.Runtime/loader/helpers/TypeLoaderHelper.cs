namespace ZSharp.Platform.Runtime.Loaders
{
    internal static class TypeLoaderHelper
    {
        public static Type LoadType(EmitLoader loader, Emit.TypeBuilder parentBuilder, IR.TypeDefinition type)
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
                IR.EnumClass def => new EnumClassLoader(loader)
                {
                    ILType = parentBuilder.DefineNestedType(
                        def.Name ?? throw new(),
                        IL.TypeAttributes.Public | IL.TypeAttributes.Sealed,
                        typeof(Enum)
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
                IR.ValueType def => throw new NotSupportedException(),
                _ => throw new NotSupportedException(),
            };
            tasks.RunUntilComplete();
            return result;
        }

        public static Type LoadType(EmitLoader loader, Emit.ModuleBuilder parentBuilder, IR.TypeDefinition type)
        {
            var tasks = new TaskManager(() => { });
            var result = type switch
            {
                IR.Class def => new ClassLoader(loader)
                {
                    ILType = parentBuilder.DefineType(
                        def.Name ?? throw new(),
                        IL.TypeAttributes.Public
                    ),
                    IRType = def,
                    Tasks = tasks
                }.Load(),
                IR.EnumClass def => new EnumClassLoader(loader)
                {
                    ILType = parentBuilder.DefineType(
                        def.Name ?? throw new(),
                        IL.TypeAttributes.Public | IL.TypeAttributes.Sealed,
                        typeof(Enum)
                    ),
                    IRType = def,
                    Tasks = tasks
                }.Load(),
                IR.Interface def => new InterfaceLoader(loader)
                {
                    ILType = parentBuilder.DefineType(
                        def.Name ?? throw new(),
                        IL.TypeAttributes.Public | IL.TypeAttributes.Interface
                    ),
                    IRType = def,
                    Tasks = tasks
                }.Load(),
                IR.ValueType def => throw new NotSupportedException(),
                _ => throw new NotSupportedException(),
            };
            tasks.RunUntilComplete();
            return result;
        }
    }
}
