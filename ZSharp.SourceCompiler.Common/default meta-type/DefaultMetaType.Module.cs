namespace ZSharp.SourceCompiler
{
    partial class DefaultMetaType
    {
        public static CompilerObject Module(IContext context, AST.Module node)
        {
            using var _ = context.Scope(out var scope);

            context = new ModuleContext()
            {
                CurrentScope = scope,
                Overrides = new()
                {
                    Parent = context.Overrides
                }
            };
        }
    }
}
