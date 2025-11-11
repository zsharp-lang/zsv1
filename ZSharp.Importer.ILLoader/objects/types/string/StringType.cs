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

        IResult<IType, Error> ICompileIRType.CompileIRType(Compiler.Compiler compiler, object? target)
            => Result<IType>.Ok(type);

        Type IILType.GetILType()
            => typeof(string);
    }
}
