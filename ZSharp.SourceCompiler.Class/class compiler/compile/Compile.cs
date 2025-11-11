namespace ZSharp.SourceCompiler.Class
{
    partial class ClassCompiler
    {
        public IResult Declare()
        {
            Error? error = null;

            if (Node.Name != string.Empty)
                if (!Interpreter
                    .Compiler
                    .CurrentContext
                    .PerformOperation<IScopeContext>(
                        scope => !scope.Add(Node.Name, Object).Error(out error)
                    )
                )
                    return Result.Error($"Could not bind function {Node.Name}: {error}");

            return Result.Ok(Object);
        }

        public void Compile()
        {
            tasks.RunUntilComplete();

            CompilerObject metaClass = Interpreter.Compiler.OOP.DefaultMetaclass;

            if (
                Node.Of is not null &&
                CompileExpression(Node.Of)
                .When(out metaClass!)
                .Error(out var error)
            )
                Error($"Failed to compile metaclass expression: {error}", new NodeLogOrigin(Node.Of));
            else if (
                Interpreter.Compiler.CG.Call(
                    metaClass, [
                        new(Interpreter.ILLoader.Expose(Interpreter.Compiler)), 
                        new(Interpreter.ILLoader.Expose(Spec))
                    ]
                )
                .When(out var metaClassResult)
                .Error(out error)
            )
                Error($"Failed to create class instance: {error}", new NodeLogOrigin(Node));
            else if (
                Interpreter.Compiler.Evaluator.Evaluate(metaClassResult!)
                .When(out var metaClassInstance)
                .Error(out error)
            )
                Error($"Failed to evaluate class instance: {error}", new NodeLogOrigin(Node));

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
