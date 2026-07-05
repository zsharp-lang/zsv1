namespace ZSharp.SourceCompiler
{
    partial class Compilers
    {
        public static IResult<CompilerObject, Error> Compile(AST.Document node)
        {
            var document = new Document();
            var context = new DocumentContext(document);

            List<CompilerObject> objects = new(node.Statements.Count);
            List<Error> errors = [];

            foreach (var statement in node.Statements)
            {
                var result = context.Compile(statement);
                if (result.IsOk)
                {
                    context.tasks.RunUntilComplete();
                    objects.Add(result.Unwrap());
                }
                else
                    errors.Add(result.UnwrapError());
                context.tasks.Reset();
            }

            if (errors.Count > 0)
                return Result.Error(errors);

            return Result.Ok(document);
        }
    }
}
