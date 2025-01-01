namespace ZSharp.Runtime.NET.IR2IL
{
    /// <summary>
    /// Wraps an IR module in a C# module.
    /// </summary>
    public class IRLoader(Context context, IR.RuntimeModule? runtimeModule = null)
    {
        private List<Action> thisPass = [], nextPass = [];

        public Context Context { get; } = context;

        public IR.RuntimeModule RuntimeModule { get; } = runtimeModule ?? IR.RuntimeModule.Standard;

        public Action<ICodeLoader, IR.VM.GetObject> GetObjectFunction { get; set; } = (_, __) => throw new NotImplementedException();

        public IL.Module LoadModule(IR.Module module)
        {
            if (Context.Cache(module, out var result))
                return result;

            var assemblybuilder = IL.Emit.AssemblyBuilder.DefineDynamicAssembly(
                new IL.AssemblyName(module.Name ?? "<AnonymousAssembly>"),
                IL.Emit.AssemblyBuilderAccess.RunAndCollect);

            result = new ModuleLoader(this, module, assemblybuilder.DefineDynamicModule(module.Name ?? "<AnonymousModule>")).Load();

            if (module.HasSubmodules)
                foreach (var submodule in module.Submodules)
                    new ModuleLoader(this, submodule, assemblybuilder.DefineDynamicModule(result.Name + '.' + (module.Name ?? "<AnonymousModule>"))).Load();

            while (nextPass.Count > 0)
            {
                (thisPass, nextPass) = (nextPass, thisPass);

                foreach (var action in thisPass)
                    action();

                thisPass.Clear();
            }

            return result;
        }

        public Type LoadType(IR.IType type)
        {
            if (Context.Cache(type, out var result))
                return result;

            return type switch
            {
                _ => throw new NotImplementedException()
            };
        }

        internal void AddToNextPass(Action action)
            => nextPass.Add(action);
    }
}
