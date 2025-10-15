namespace ZSharp.SourceCompiler.Module
{
    partial class ModuleCompiler
    {
        private Result Compile(AST.ExpressionStatement expressionStatement)
        {
            var valueObjectResult = Compile(expressionStatement.Expression);

            if (
                valueObjectResult
                .When(out var valueObject)
                .Error(out var error)
            )
                return Result.Error(error);

            Interpreter.Compiler.Evaluator.Evaluate(valueObject!);
            return Result.Ok(Objects.EmptyObject.Empty); // it's most likely that the evaluator itself needs
            // to return the empty object, although we do need to first cast to void. right now it returns null
            // but we can't trust the evaluator saying that null is not valid because it's the only valid value
            // for void type.
        }
    }
}
