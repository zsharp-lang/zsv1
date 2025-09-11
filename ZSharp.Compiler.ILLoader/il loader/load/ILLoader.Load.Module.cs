namespace ZSharp.Compiler.ILLoader
{
    partial class ILLoader
    {
        private readonly Dictionary<IL.Module, CompilerObject> moduleCache = [];

        public CompilerObject LoadModule(IL.Module module)
        {
            if (!moduleCache.TryGetValue(module, out var @object))
                @object = moduleCache[module] = DispatchLoadModule(module);

            return @object;
        }

        private CompilerObject DispatchLoadModule(IL.Module module)
        {
            return new Objects.Module(module, this);
        }
    }
}
