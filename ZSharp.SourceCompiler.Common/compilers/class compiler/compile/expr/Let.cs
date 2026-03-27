namespace ZSharp.SourceCompiler.Class
{
    partial class ClassCompiler
    {
        private IResult Compile(AST.LetExpression let)
        {
            var result = new Objects.Local()
            {
                Name = let.Name
            };

            Spec.Content.Add(result);

            tasks.AddTask(() =>
            {
                if (let.Value is not null)
                    if (
                        Compile(let.Value)
                        .When(out var value)
                        .Error(out var error)
                    )
                        Error($"Could not compile value for local '{let.Name}': {error}", let.Value);
                    else result.Value = value;

                if (let.Type is not null)
                {
                    if (
                        Compile(let.Type)
                        .When(out var type)
                        .Error(out var error)
                    )
                        Error($"Could not compile type for local '{let.Name}': {error}", let.Type);
                    else result.Type = type;
                } else if (
                    result.Value is not null
                )
                    if (
                        Interpreter.Compiler.TS.TypeOf(result.Value)
                        .Ok(out var inferredType)
                    ) result.Type = inferredType;
            });

            return Result.Ok(result);
        }
    }
}
