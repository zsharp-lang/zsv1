using ZSharp.SourceCompiler.HIR.BuiltIn;

namespace ZSharp.SourceCompiler
{
    partial class DefaultMetaType
    {
        public static HIR.IDefinition Class(MetaContext<AST.TypeDefinition> context)
        {
            if (context.Node.GenericParameters is not null)
                return CreateGenericClass(context);

            return CreateConcreteClass(context);
        }

        private static HIR.BuiltIn.Class CreateConcreteClass(MetaContext<AST.TypeDefinition> context)
        {
            context.AddBuildTask(
                () =>
                {
                    context.DefinitionCompiler.RegisterHandler<AST.LetExpression>(
                        node =>
                        {

                        }
                    );

                    using var _ = context.Scope();



                    return;
                }
            );

            return new()
            {
                Name = context.Node.Name
            };
        }

        private static GenericClass CreateGenericClass(MetaContext<AST.TypeDefinition> context)
        {
            context.AddBuildTask(
                () =>
                {
                    return;
                }
            );

            return new()
            {
                Name = context.Node.Name
            };
        }
    }
}
