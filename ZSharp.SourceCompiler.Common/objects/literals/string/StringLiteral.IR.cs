namespace ZSharp.SourceCompiler
{
    partial class StringLiteral
        : ICompileIRCode
    {
        Result<IRCode> ICompileIRCode.CompileIRCode(Compiler.Compiler compiler, object? target)
            => Result<IRCode>.Ok(
                new([
                    new IR.VM.PutString(value)
                ])
                {
                    Types = [compiler.IR.RuntimeModule.TypeSystem.String]
                }
            );
    }
}
