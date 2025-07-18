using ZSharp.Compiler;

namespace ZSharp.Objects
{
    public sealed class PointerType(IType elementType)
        : CompilerObject
        , IType
        , ICompileIRType<IR.ConstructedClass>
    {
        public IType ElementType { get; set; } = elementType;

        IR.ConstructedClass ICompileIRType<IR.ConstructedClass>.CompileIRType(Compiler.Compiler compiler)
        {
            var elementType = compiler.IR.CompileType(ElementType).Unwrap();

            return new(compiler.TypeSystem.PointerType.IR)
            {
                Arguments = [elementType]
            };
        }
    }
}
