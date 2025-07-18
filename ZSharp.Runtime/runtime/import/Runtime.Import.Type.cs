using System.Reflection.Metadata;

namespace ZSharp.Runtime
{
    partial class Runtime
    {
        public Type ImportType(IR.IType type)
        {
            if (_typeCache.Cache(type, out var result)) return result;

            return type switch
            {
                IR.OOPTypeReference reference => ImportTypeReference(reference),
                _ => throw new NotImplementedException()
            };
        }

        public Type ImportTypeDefinition(IR.OOPType def)
        {
            if (!_typeDefCache.TryGetValue(def, out var result))
                result = _typeDefCache[def] = LoadType(def);

            return result;
        }

        public Type ImportTypeReference(IR.OOPTypeReference @ref)
        {
            if (
                @ref.Definition == RuntimeModule.TypeSystem.Array
            ) return ImportArrayType(@ref);
            if (
                @ref.Definition == RuntimeModule.TypeSystem.Pointer
            ) return ImportPointerType(@ref);
            if (
                @ref.Definition == RuntimeModule.TypeSystem.Reference
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
