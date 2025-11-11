namespace ZSharp.SourceCompiler.Module
{
    partial class ClassCompiler
    {
        public IResult Declare()
        {
            Error? error = null;

            if (Object.Name != string.Empty)
                if (!Interpreter
                    .Compiler
                    .CurrentContext
                    .PerformOperation<IScopeContext>(
                        scope => !scope.Add(Object.Name, Object).Error(out error)
                    )
                )
                    return Result.Error($"Could not bind function {Node.Name}: {error}");

            return Result.Ok(Object);
        }

        public void Compile()
        {
            tasks.RunUntilComplete();

            if (logs.Count > 0)
            {
                // TODO: Aggregate errors properly

                var sb = new System.Text.StringBuilder();

                foreach (var log in logs)
                    sb.AppendLine(log.ToString());

                Interpreter.Log.Error(sb.ToString(), Node);
            }
        }

        private void InitCompile()
        {
            foreach (var statement in Node.Content?.Statements ?? [])
                if (Compile(statement).Error(out var error))
                    Error(
                        $"Failed to compile statement: {error}",
                        new NodeLogOrigin(statement)
                    );
        }

        private IResult Compile(AST.Statement statement)
            => statement switch
            {
                AST.DefinitionStatement definitionStatement => Compile(definitionStatement),
                AST.ExpressionStatement expressionStatement => Compile(expressionStatement),
                _ => Result.Error($"Unknown statement type: {statement.GetType().Name}")
            };
    }
}
