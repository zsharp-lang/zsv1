using ZSharp.Compiler;
using ZSharp.IR;

namespace ZSharp.Importer.ILLoader.Objects
{
    internal sealed class StringType(TypeReference type)
        : CompilerObject
        , ICompileIRType
        , IILType
        , IReferenceType
    {
        private readonly TypeReference type = type;

        Result<IType> ICompileIRType.CompileIRType(Compiler.Compiler compiler)
            => Result<IType>.Ok(type);

        Type IILType.GetILType()
            => typeof(string);
    }
}
