namespace ZSharp.Compiler.ILLoader
{
    partial class ILLoader
    {
        public IType LoadModifiedType(Type type)
        {
            if (type.IsArray) return LoadArrayType(type);
            if (type.IsPointer) return LoadPointerType(type);
            if (type.IsByRef) return LoadReferenceType(type);

            throw new NotSupportedException();
        }

        public IType LoadArrayType(Type array)
        {
            throw new NotImplementedException();
        }

        public IType LoadPointerType(Type pointer)
        {
            throw new NotImplementedException();
        }

        public IType LoadReferenceType(Type reference)
        {
            throw new NotImplementedException();
        }
    }
}
