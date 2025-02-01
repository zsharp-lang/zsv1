using ZSharp.Compiler;

namespace ZSharp.Objects
{
    public sealed class StringType(IR.ConstructedClass stringType, CompilerObject type)
        : CompilerObject
        , ICompileIRType
        , ICTCallable
    {
        public IR.ConstructedClass IR { get; } = stringType;

        public CompilerObject Type { get; } = type;

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
