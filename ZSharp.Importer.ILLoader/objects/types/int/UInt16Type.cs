using ZSharp.Compiler;
using ZSharp.IR;

namespace ZSharp.Importer.ILLoader.Objects
{
    public sealed class UInt16Type(IType type)
        : CompilerObject
        , ICompileIRType
    {
        private readonly IType type = type;

        IResult<IType, Error> ICompileIRType.CompileIRType(Compiler.Compiler compiler, object? target)
            => Result<IType>.Ok(type);
    }
}
