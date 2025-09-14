namespace ZSharp.Platform.Runtime
{
    partial class Runtime
    {
        public IL.Module ImportModule(IR.Module module)
        {
            if (!_moduleCache.TryGetValue(module, out var result))
                result = _moduleCache[module] = LoadModule(module);

            return result;
        }
    }
}
