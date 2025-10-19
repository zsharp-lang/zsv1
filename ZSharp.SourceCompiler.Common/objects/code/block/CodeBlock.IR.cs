
namespace ZSharp.SourceCompiler
{
    partial class CodeBlock
        : ICompileIRCode
    {
        IResult<IRCode, Error> ICompileIRCode.CompileIRCode(Compiler.Compiler compiler, object? target)
        {
            var code = new IRCode();

            foreach (var statement in Content)
            {
                if (
                    compiler.IR.CompileCode(statement, target)
                    .When(out var statementCode)
                    .Error(out var error)
                ) return Result<IRCode>.Error(error);
                code.Instructions.AddRange(statementCode!.Instructions);
                code.Types.AddRange(statementCode.Types);
            }

            return Result<IRCode>.Ok(code);
        }
    }
}
