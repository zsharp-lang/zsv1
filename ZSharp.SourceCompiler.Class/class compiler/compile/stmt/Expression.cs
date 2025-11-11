namespace ZSharp.SourceCompiler.Class
{
    partial class ClassCompiler
    {
        private IResult Compile(AST.ExpressionStatement expressionStatement)
        {
            var valueObjectResult = Compile(expressionStatement.Expression);

            if (
                valueObjectResult
                .When(out var valueObject)
                .Error(out var error)
            )
                return Result.Error(error);

            Interpreter.Compiler.Evaluator.Evaluate(valueObject!);
            return Result.Ok(EmptyObject.Empty); // See comment in ModuleCompiler.Compile.Expression.cs
        }
    }
}
