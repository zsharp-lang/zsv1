namespace ZSharp.Platform.Runtime
{
    partial class Runtime
    {
        public IL.FieldInfo ImportField(IR.Field field)
        {
            if (!_fieldCache.TryGetValue(field, out var result))
                throw new InvalidOperationException(
                    $"Field {field.Name} in type {field.Owner?.Name ?? "<???>"} is not loaded"
                );

            return result;
        }

        public IL.FieldInfo ImportFieldReference(IR.FieldReference @ref)
        {
            var type = ImportType(@ref.OwningType);

            var def = ImportField(@ref.Field);

            if (!type.IsGenericType)
                return def;

            if (type.GetGenericTypeDefinition() is Emit.TypeBuilder typeBuilder)
                if (!typeBuilder.IsCreated())
                    return Emit.TypeBuilder.GetField(type, def);

            return IL.FieldInfo.GetFieldFromHandle(
                def.FieldHandle,
                type.TypeHandle
            );
        }
    }
}
