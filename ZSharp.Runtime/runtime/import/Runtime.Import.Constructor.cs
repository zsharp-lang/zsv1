namespace ZSharp.Runtime
{
    partial class Runtime
    {
        public IL.ConstructorInfo ImportConstructor(IR.Constructor constructor)
        {
            var function = constructor.Method.UnderlyingFunction;
            if (!_functionCache.TryGetValue(function, out var result))
                throw new InvalidOperationException(
                    $"Constructor{' ' + constructor.Name ?? string.Empty} in type {constructor.Method.Owner} is not loaded"
                );
            if (result is not IL.ConstructorInfo info)
                throw new InvalidOperationException();

            return info;
        }

        public IL.ConstructorInfo ImportConstructorReference(IR.ConstructorReference @ref)
        {
            var type = ImportType(@ref.OwningType);

            var def = ImportConstructor(@ref.Constructor);

            if (!type.IsGenericType)
                return def;

            if (type.GetGenericTypeDefinition() is Emit.TypeBuilder typeBuilder)
                if (!typeBuilder.IsCreated())
                    return Emit.TypeBuilder.GetConstructor(type, def);
            try
            {
                def = (IL.ConstructorInfo)(IL.MethodBase.GetMethodFromHandle(
                    def.MethodHandle,
                    type.TypeHandle
                ) ?? throw new("Could not create constructor from method handle"));
            }
            catch (NotSupportedException)
            {
                def = Emit.TypeBuilder.GetConstructor(type, def);
            }

            return def;
        }
    }
}
