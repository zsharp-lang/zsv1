namespace ZSharp.Importer.ILLoader
{
    partial class ILLoader
    {
        public CompilerObject LoadModifiedType(Type type)
        {
            if (type.IsArray) return LoadArrayType(type);
            if (type.IsPointer) return LoadPointerType(type);
            if (type.IsByRef) return LoadReferenceType(type);

            throw new NotSupportedException();
        }

        public CompilerObject LoadArrayType(Type array)
            => LoadModifiedType(
                TypeSystem.Array,
                array.GetElementType() ?? throw new ArgumentException("Type must have an element type", nameof(array))
            );

        public CompilerObject LoadPointerType(Type pointer)
        => LoadModifiedType(
                TypeSystem.Array,
                pointer.GetElementType() ?? throw new ArgumentException("Type must have an element type", nameof(pointer))
            );

        public CompilerObject LoadReferenceType(Type reference)
        => LoadModifiedType(
                TypeSystem.Array,
                reference.GetElementType() ?? throw new ArgumentException("Type must have an element type", nameof(reference))
            );

        private CompilerObject LoadModifiedType(CompilerObject modifier, Type inner)
        {
            if (!modifier.Is<ITypeModifier>(out var typeModifier))
                throw new NotSupportedException();

            var typeCO = LoadType(inner);

            return typeModifier.Modify(typeCO);
        }
    }
}
