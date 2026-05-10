namespace ZSharp.SourceCompiler
{
    public static class Compiler
    {
        public static IResult Compile(this IContext context, AST.Definition node)
        {
            if (node.MetaType is null)
                return CompileFromOverrides(context, node).When(co =>
                {
                    context.Define(co);
                    return co;
                });

            if (
                context.Compile(node.MetaType)
                .When(out var metaType)
                .Error(out var error)
            ) return Result.Error(error);

            if (
                Core.Runtime.ModuleScope.RTLoader.Load(context)
                .When(out var contextCO)
                .Error(out error)
                ||
                Core.Runtime.ModuleScope.RTLoader.Load(node)
                .When(out var nodeCO)
                .Error(out error)
                ) return Result.Error(error);

            if (
                Core.Compiler.CO.CG.ModuleScope.CG.Call(
                    metaType!,
                    [
                        new(contextCO!),
                        new(nodeCO!)
                    ]
                )
                .When(out var result)
                .Error(out error)
            ) return Result.Error(error);

            return Core.Compiler.CO.Evaluator.ModuleScope.Evaluator.Evaluate(result!);
        }

        public static IResult Compile(this IContext context, AST.Expression node)
        {
            if (node is AST.Definition definition)
                return Compile(context, definition);

            return CompileFromOverrides(context, node);
        }

        public static IResult Compile(this IContext context, AST.Node node)
            => node switch
            {
                AST.Definition definition => Compile(context, definition),
                AST.Expression expression => Compile(context, expression),
                AST.Statement statement => Compile(context, statement),
                _ => CompileFromOverrides(context, node)
            };

        public static IResult Compile(this IContext context, AST.Statement node)
            => CompileFromOverrides(context, node);

        private static IResult CompileFromOverrides(IContext context, AST.Node node)
            => context.Overrides.Cache(node.GetType(), out var fn)
                ? fn(context, node)
                : throw new ArgumentException($"Unsupported node type: {node.GetType().Name}");
    }
}
