namespace ZSharp.Importer.ILLoader
{
    partial class ILLoader
    {
        private readonly Dictionary<Type, CompilerObject> typeCache = [];

        public CompilerObject LoadType(Type type)
        {
            if (!typeCache.TryGetValue(type, out var @object))
                @object = typeCache[type] = DispatchLoadType(type);

            return @object;
        }

        private CompilerObject DispatchLoadType(Type type)
        {
            if (type.IsConstructedGenericType)
                return LoadConstructedGenericType(type);

            if (type.IsClass) return LoadClass(type);
            if (type.IsInterface) return LoadInterface(type);
            if (type.IsEnum) return LoadEnum(type);
            if (type.IsValueType) return LoadStruct(type);

            if (type.HasElementType)
                return LoadModifiedType(type);

            throw new NotSupportedException();
        }
    }
}
