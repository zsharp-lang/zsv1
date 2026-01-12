namespace ZSharp.SourceCompiler.Module
{
    partial class ClassCompiler
    {
        public IResult Declare()
        {
            tasks.RunUntilComplete();

            if (logs.Count > 0)
            {
                // TODO: Aggregate errors properly

                var sb = new System.Text.StringBuilder();

                foreach (var log in logs)
                    sb.AppendLine(log.ToString());

                return Result.Error(sb.ToString());
            }
                

            return Result.Ok(Object);
        }

        private void InitCompile()
        {
            foreach (var statement in Node.Body)
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
