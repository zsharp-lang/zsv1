namespace ZSharp.SourceCompiler
{
    partial class DefaultMetaType
    {
        public static HIR.BuiltIn.Function Function(MetaContext<AST.Function> context)
        {
            var function = new HIR.BuiltIn.Function()
            {
                Name = context.Node.Name
            };

            context.AddBuildTask(
                () =>
                {
                    using var _ = context.ASTCompiler.Context();
                    using var __ = context.Scope();

                    context.ASTCompiler.RegisterHandler<AST.Function>(
                        Closure
                    );

                    // compile body as top level and collect all definitions
                    var tlCompiler = new Interpreter()
                    {
                        CompileStatement = ,
                        RootInterpreter = ,
                    };

                    var errors = context.Node.Body.Select(tlCompiler.Execute).OfType<Error>().ToArray();

                    if (errors.Length != 0)
                        return Result<HIR.IDefinition>.Error(errors);

                    context.Build();
                }
            );

            context.AddValidationTask(context.Validate);

            return function;
        }
    }
}
