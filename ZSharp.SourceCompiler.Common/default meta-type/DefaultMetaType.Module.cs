namespace ZSharp.SourceCompiler
{
    partial class DefaultMetaType
    {
        public static HIR.BuiltIn.Module Module(MetaContext<AST.Module> context)
        {
            var module = new HIR.BuiltIn.Module()
            {
                Name = context.Node.Name
            };

            context.AddBuildTask(
                () =>
                {
                    using var _ = context.ASTCompiler.Context();

                    var CompileDefinition = context.ASTCompiler.CompileDefinition;
                    context.ASTCompiler.CompileDefinition =
                        def =>
                        {
                            var definition = CompileDefinition(def);
                            module.Items.Add(definition);
                            return definition;
                        };

                    context.ASTCompiler.RegisterHandler<AST.Function>(
                        Function
                    );

                    // compile body as top level and collect all definitions
                    var tlCompiler = new Interpreter()
                    {
                        CompileStatement = context.ASTCompiler.Compile,
                        RootInterpreter = context.Interpreter,
                    };

                    var errors = context.Node.Body.Select(tlCompiler.Execute).OfType<Error>().ToArray();

                    if (errors.Length != 0)
                        return Result<HIR.IDefinition>.Error(errors);

                    context.Build();
                }
            );

            context.AddValidationTask(context.Validate);

            return module;
        }
    }
}
