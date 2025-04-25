using ZSharp.Compiler;

namespace ZSharp.Objects
{
    internal sealed class Type(IRType type)
        : CompilerObject
        //, ICTReadable
        , ICompileIRType
        , IType
    {
        private readonly IRType type = type;
        //private readonly IRObject ir = type as IRObject ?? throw new();

        //CompilerObject ITyped.Type => this;

        IRType ICompileIRType.CompileIRType(Compiler.Compiler compiler)
            => type;

        //public IRCode Read(Compiler.Compiler compiler)
        //=> new([
        //        new IR.VM.GetObject(ir)
        //    ])
        //    {
        //        MaxStackSize = 1,
        //        Types = [this]
        //    };
    }
}
