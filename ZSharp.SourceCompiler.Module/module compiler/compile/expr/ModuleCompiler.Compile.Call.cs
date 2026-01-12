namespace ZSharp.SourceCompiler.Module
{
    partial class ModuleCompiler
    {
        private IResult Compile(AST.CallExpression call)
        {
            var calleeResult = Compile(call.Callee);

            if (
                calleeResult
                .When(out var callee)
                .Error(out var errorMessage)
            ) return Result<CompilerObject>.Error(
                $"Failed to compile callee: {errorMessage}"
            );

            var argumentResults = call.Arguments
                .Select<AST.CallArgument, IResult<Argument, Error>>(arg =>
                {
                    if (
                        Compile(arg.Value)
                        .When(out var argValue)
                        .Error(out var error)
                    ) return Result<Argument>.Error(error);

                    return Result<Argument>.Ok(
                        new(
                            arg.Name,
                            argValue!
                        )
                    );
                })
                .ToList();

            if (argumentResults.Any(r => r.IsError))
                return Result<CompilerObject>.Error(
                    $"Failed to compile arguments: {
                        (string.Join(", ", Enumerable.Where(argumentResults, (r => r.IsError)).Select(r => r.UnwrapError())))
                    }"
                );

            return Interpreter.Compiler.CG.Call(callee!, [.. argumentResults.Select(r => r.Unwrap())]);
        }
    }
}
