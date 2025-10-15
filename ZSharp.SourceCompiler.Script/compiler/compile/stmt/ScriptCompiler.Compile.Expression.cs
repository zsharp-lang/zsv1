namespace ZSharp.SourceCompiler.Script
{
    partial class ScriptCompiler
    {
        private void Compile(AST.ExpressionStatement expressionStatement)
        {
            var valueObjectResult = Compile(expressionStatement.Expression);

            if (
                valueObjectResult
                .When(out var valueObject)
                .Error(out var errorMessage)
            )
            {
                Interpreter.Log.Error(
                    $"Failed to compile expression: {errorMessage}",
                    new NodeLogOrigin(expressionStatement)
                );

                return;
            }

            Interpreter.Compiler.Evaluator.Evaluate(valueObject!);
            // See comment in ZSharp.SourceCompiler.Module/module compiler/compile/stmt/ModuleCompiler.Compile.Expression.cs
        }
    }
}
