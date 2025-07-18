namespace ZSharp.Compiler.ILLoader
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
        {
            throw new NotImplementedException();
        }

        public CompilerObject LoadPointerType(Type pointer)
        {
            throw new NotImplementedException();
        }

        public CompilerObject LoadReferenceType(Type reference)
        {
            throw new NotImplementedException();
        }
    }
}
