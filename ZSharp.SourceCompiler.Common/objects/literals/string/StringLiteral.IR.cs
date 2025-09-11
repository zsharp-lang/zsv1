namespace ZSharp.SourceCompiler
{
    partial class StringLiteral
        : ICompileIRCode
    {
        Result<IRCode> ICompileIRCode.CompileIRCode(Compiler.IR ir, object? target)
            => Result<IRCode>.Ok(
                new([
                    new IR.VM.PutString(value)
                ])
                {
                    Types = [ir.RuntimeModule.TypeSystem.String]
                }
            );
    }
}
