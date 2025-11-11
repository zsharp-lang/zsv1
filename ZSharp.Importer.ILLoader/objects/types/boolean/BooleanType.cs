using ZSharp.Compiler;
using ZSharp.IR;

namespace ZSharp.Importer.ILLoader.Objects
{
    public sealed class BooleanType(TypeReference type)
        : CompilerObject
        , ICompileIRType
    {
        private readonly TypeReference type = type;

        IResult<IType, Error> ICompileIRType.CompileIRType(Compiler.Compiler compiler, object? target)
            => Result<IType>.Ok(type);
    }
}
