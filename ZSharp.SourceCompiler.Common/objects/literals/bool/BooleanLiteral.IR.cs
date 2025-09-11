namespace ZSharp.SourceCompiler
{
    partial class BooleanLiteral
        : ICompileIRCode
    {
        Result<IRCode> ICompileIRCode.CompileIRCode(Compiler.IR ir, object? target)
            => Result<IRCode>.Ok(
                new([
                    new IR.VM.PutBoolean(value)
                ])
                {
                    Types = [ir.RuntimeModule.TypeSystem.Boolean]
                }
            );
    }
}
