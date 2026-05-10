namespace ZSharp.SourceCompiler
{
    partial class BooleanLiteral
        : ICompileIRCode
    {
        IResult<IRCode, Error> ICompileIRCode.CompileIRCode(ZSharp.Compiler.Compiler compiler, object? target)
            => Result<IRCode>.Ok(
                new([
                    new IR.VM.PutBoolean(value)
                ])
                {
                    Types = [compiler.IR.RuntimeModule.TypeSystem.Boolean]
                }
            );
    }
}
