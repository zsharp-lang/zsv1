namespace ZSharp.SourceCompiler
{
    partial class StringLiteral
        : ICompileIRCode
    {
        IResult<IRCode, Error> ICompileIRCode.CompileIRCode(Compiler.Compiler compiler, object? target)
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
