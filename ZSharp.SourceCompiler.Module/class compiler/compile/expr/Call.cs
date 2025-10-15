namespace ZSharp.SourceCompiler.Module
{
    partial class ClassCompiler
    {
        private Result<CompilerObject> Compile(AST.CallExpression call)
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
                .Select(arg =>
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
                        (string.Join(", ", Enumerable.Where<Result<Argument>>(argumentResults, (Func<Result<Argument>, bool>)(r => r.IsError)).Select(r => r.Error(out var error) ? error : throw new())))
                    }"
                );

            return Interpreter.Compiler.CG.Call(callee!, [.. argumentResults.Select(r => r.Unwrap())]);
        }
    }
}
