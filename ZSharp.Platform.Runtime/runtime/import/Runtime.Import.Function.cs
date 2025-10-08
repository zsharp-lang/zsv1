namespace ZSharp.Platform.Runtime
{
    partial class Runtime
    {
        public IL.MethodInfo ImportFunction(IR.Function function)
        {
            if (!_functionCache.TryGetValue(function, out var result))
                result = _functionCache[function] = LoadFunction(function);
            if (result is not IL.MethodInfo info)
                throw new InvalidOperationException();

            return info;
        }

        public IL.MethodInfo ImportConstructedFunction(IR.ConstructedFunction constructedFunction)
        {
            var def = ImportFunction(constructedFunction.Function);

            return def.MakeGenericMethod([.. constructedFunction.Arguments.Select(ImportType)]);
        }
    }
}
