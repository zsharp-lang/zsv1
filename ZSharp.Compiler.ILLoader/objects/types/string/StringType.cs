using ZSharp.IR;

namespace ZSharp.Compiler.ILLoader.Objects
{
    public sealed class StringType(OOPTypeReference type)
        : CompilerObject
        , ICompileIRType
    {
        private readonly OOPTypeReference type = type;

        Result<IType> ICompileIRType.CompileIRType(IR ir)
            => Result<IType>.Ok(type);
    }
}
