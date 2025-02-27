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

            if (module.Initializer is not null)
                result
                    .GetType(Constants.GlobalsTypeName)!
                    .GetMethod(Context.Cache(module.Initializer)!.Name, [])!
                    .Invoke(null, null);

            return result;
        }

        public Type LoadType(IR.IType type)
        {
            if (Context.Cache(type, out var result))
                return result;

            return type switch
            {
                IR.ConstructedClass constructedClass => LoadType(constructedClass),
                _ => throw new NotImplementedException()
            };
        }

        public Type LoadType(IR.ConstructedClass constructedClass)
        {
            var innerClass = Context.Cache(constructedClass.Class);

            if (innerClass is null)
                throw new();

            if (constructedClass.Arguments.Count == 0)
                return innerClass;

            return innerClass.MakeGenericType([
                .. constructedClass.Arguments.Select(LoadType)
            ]);
        }

        internal void AddToNextPass(Action action)
            => nextPass.Add(action);
    }
}
