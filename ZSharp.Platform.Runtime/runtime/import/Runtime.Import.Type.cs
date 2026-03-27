using System.Reflection.Metadata;

namespace ZSharp.Platform.Runtime
{
    partial class Runtime
    {
        public Type ImportType(IR.IType type)
        {
            if (_typeCache.Cache(type, out var result)) return result;

            return type switch
            {
                IR.TypeReference reference => ImportTypeReference(reference),
                _ => throw new NotImplementedException()
            };
        }

        public Type ImportTypeDefinition(IR.TypeDefinition def)
        {
            if (!_typeDefCache.TryGetValue(def, out var result))
                result = _typeDefCache[def] = LoadType(def);

            return result;
        }

        public Type ImportTypeReference(IR.TypeReference @ref)
        {
            if (
                @ref.Definition == TypeSystem.Array ||
                @ref.Definition == TypeSystem.CoreTypes.Array.Definition
            ) return ImportArrayType(@ref);
            if (
                @ref.Definition == TypeSystem.Pointer
            ) return ImportPointerType(@ref);
            if (
                @ref.Definition == TypeSystem.Reference
            ) return ImportReferenceType(@ref);

            var type = ImportTypeDefinition(@ref.Definition);

            List<Type> genericArguments = [];

            if (@ref.OwningType is not null)
                genericArguments.AddRange(ImportType(@ref.OwningType).GetGenericArguments());

            if (@ref is IR.ConstructedType constructedType)
                genericArguments.AddRange(constructedType.Arguments.Select(ImportType));

            if (genericArguments.Count == 0)
                return type;

            return type.MakeGenericType([.. genericArguments]);
        }
    }
}
