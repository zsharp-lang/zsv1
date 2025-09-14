namespace ZSharp.Platform.Runtime
{
    partial class Runtime
    {
        private Type ImportArrayType(IR.OOPTypeReference @ref)
        {
            var con = (IR.ConstructedType)@ref;
            var elementType = ImportType(con.Arguments[0]);

            return elementType.MakeArrayType();
        }

        private Type ImportPointerType(IR.OOPTypeReference @ref)
        {
            var con = (IR.ConstructedType)@ref;
            var elementType = ImportType(con.Arguments[0]);

            return elementType.MakePointerType();
        }

        private Type ImportReferenceType(IR.OOPTypeReference @ref)
        {
            var con = (IR.ConstructedType)@ref;
            var elementType = ImportType(con.Arguments[0]);

            return elementType.MakeByRefType();
        }
    }
}
