namespace ZSharp.SourceCompiler
{
    partial class FunctionBodyCompiler
    {
        private IResult Compile(AST.BlockStatement block)
        {
            var result = new CodeBlock();

            foreach (var statement in block.Statements ?? [])
                if (Compile(statement).When(out var @object).Error(out var error))
                    Interpreter.Log.Error($"{error}", statement);
                else result.Content.Add(@object!);

            return Result.Ok(result);
        }
    }
}
