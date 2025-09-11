using ZSharp.IR;

namespace ZSharp.Compiler.ILLoader.Objects
{
    public sealed class UInt8Type(IType type)
        : CompilerObject
        , ICompileIRType
    {
        private readonly IType type = type;

        Result<IType> ICompileIRType.CompileIRType(IR ir)
            => Result<IType>.Ok(type);
    }
}
