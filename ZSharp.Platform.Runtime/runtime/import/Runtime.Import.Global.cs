namespace ZSharp.Platform.Runtime
{
    partial class Runtime
    {
        public IL.FieldInfo ImportGlobal(IR.Global global)
        {
            if (!_globalCache.TryGetValue(global, out var result))
                throw new InvalidOperationException(
                    $"Global {global.Name} in module {global.Module?.Name ?? "<???>"} is not loaded"
                );

            return result;
        }
    }
}
