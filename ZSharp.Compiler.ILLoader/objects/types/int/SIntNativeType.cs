using ZSharp.IR;

namespace ZSharp.Compiler.ILLoader.Objects
{
    public sealed class SIntNativeType(IType type)
        : CompilerObject
        , ICompileIRType
    {
        private readonly IType type = type;

        Result<IType> ICompileIRType.CompileIRType(IR ir)
            => Result<IType>.Ok(type);
    }
}
