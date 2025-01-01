using ZSharp.Compiler;
using ZSharp.IR;

namespace ZSharp.Objects
{
    public sealed class RawType(IRType type, CompilerObject metaType)
        : CompilerObject
        , ICTReadable
        , ICompileIRType
    {
        private IRType type = type;

        public CompilerObject Type { get; internal set; } = metaType;

        public IType CompileIRType(Compiler.Compiler compiler)
            => type;

        public IRCode Read(Compiler.Compiler compiler)
            => type is IRObject ir ? new([
                new IR.VM.GetObject(ir)
            ])
            {
                MaxStackSize = 1,
                Types = [Type]
            } : throw new();
    }
}
