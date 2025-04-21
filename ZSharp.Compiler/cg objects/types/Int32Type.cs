using CommonZ.Utils;
using ZSharp.Compiler;

namespace ZSharp.Objects
{
    public sealed class Int32Type(IR.OOPTypeReference<IR.Class> ir, CompilerObject type)
        : CompilerObject
        , ICompileIRType
        , ICTGetMember<MemberName>
    {
        public IR.OOPTypeReference<IR.Class> IR { get; } = ir;

        public CompilerObject Type { get; } = type;

        public Mapping<string, CompilerObject> Members { get; } = [];

        public IRType CompileIRType(Compiler.Compiler compiler)
            => IR;

        public CompilerObject Member(Compiler.Compiler compiler, string member)
            => Members[member];
    }
}
