namespace ZSharp.Runtime.Loaders
{
    partial class EmitLoader
    {
        public Type LoadType(IR.OOPType type)
        {
            var tasks = new TaskManager(() => { });
            var result = type switch
            {
                IR.Class def => new ClassLoader(this)
                {
                    ILType = StandaloneModule.DefineType(
                        def.Name ?? throw new(),
                        IL.TypeAttributes.Public
                    ),
                    IRType = def,
                    Tasks = tasks
                }.Load(),
                _ => throw new NotSupportedException(),
            };
            tasks.RunUntilComplete();
            return result;
        }
    }
}
