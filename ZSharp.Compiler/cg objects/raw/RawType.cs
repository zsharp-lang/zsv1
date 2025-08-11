using ZSharp.Compiler;

namespace ZSharp.Objects
{
    public sealed class RawType(IRType type, IType metaType)
        : CompilerObject
        , ICTReadable
        , ICompileIRType
        , IType
    {
        private IRType type = type;

        public IType Type { get; internal set; } = metaType;

        public IRType CompileIRType(Compiler.Compiler compiler)
            => type;

        public IRCode Read(Compiler.Compiler compiler)
            => type is IR.IRDefinition ir ? new([
                new IR.VM.GetObject(ir)
            ])
            {
                MaxStackSize = 1,
                Types = [Type]
            } : throw new();
    }
}
