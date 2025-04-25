using ZSharp.Compiler;

namespace ZSharp.Objects
{
    public sealed class StringType(IR.OOPTypeReference<IR.Class> stringType, IType type)
        : CompilerObject
        , ICompileIRType
        , ICTCallable
        , IType
    {
        public IR.OOPTypeReference<IR.Class> IR { get; } = stringType;

        public IType Type { get; } = type;

        public new CompilerObject ToString { get; set; } = null!;

        public CompilerObject Call(Compiler.Compiler compiler, Argument[] arguments)
        {
            if (arguments.Length != 1)
                throw new();

            return compiler.Call(ToString, arguments);
        }

        public IRType CompileIRType(Compiler.Compiler compiler)
            => IR;
    }
}
