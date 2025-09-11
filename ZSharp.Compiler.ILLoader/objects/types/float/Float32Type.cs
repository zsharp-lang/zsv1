using ZSharp.IR;

namespace ZSharp.Compiler.ILLoader.Objects
{
    public sealed class Float32Type(OOPTypeReference type)
        : CompilerObject
        , ICompileIRType
    {
        private readonly OOPTypeReference type = type;

        Result<IType> ICompileIRType.CompileIRType(IR ir)
            => Result<IType>.Ok(type);
    }
}
