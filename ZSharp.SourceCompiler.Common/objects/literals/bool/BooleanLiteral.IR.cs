namespace ZSharp.SourceCompiler
{
    partial class BooleanLiteral
        : ICompileIRCode
    {
        Result<IRCode> ICompileIRCode.CompileIRCode(Compiler.Compiler compiler, object? target)
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
