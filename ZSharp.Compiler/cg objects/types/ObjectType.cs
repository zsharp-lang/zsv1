using ZSharp.Compiler;

namespace ZSharp.Objects
{
    public sealed class ObjectType(IR.OOPTypeReference<IR.Class> stringType, IType type)
        : CompilerObject
        , IClass
        , ICompileIRType
    {
        public IR.OOPTypeReference<IR.Class> IR { get; } = stringType;

        public IType Type { get; } = type;
        public string Name
        {
            get => IR.Definition.Name!;
            set => throw new InvalidOperationException();
        }
        public IClass? Base
        {
            get => null;
            set => throw new InvalidOperationException();
        }

        public IRType CompileIRType(Compiler.Compiler compiler)
            => IR;
    }
}
