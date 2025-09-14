using ZSharp.Compiler;
using ZSharp.IR;

namespace ZSharp.Importer.ILLoader.Objects
{
    public sealed class UInt8Type(IType type)
        : CompilerObject
        , ICompileIRType
    {
        private readonly IType type = type;

        Result<IType> ICompileIRType.CompileIRType(Compiler.IR ir)
            => Result<IType>.Ok(type);
    }
}
