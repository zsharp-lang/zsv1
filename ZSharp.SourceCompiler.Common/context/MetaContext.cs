using CommonZ.Utils;
using ZSharp.Logging;

namespace ZSharp.SourceCompiler
{
    public sealed class MetaContext<T>
        where T : AST.Definition
    {
        private readonly List<Action> buildTasks = [];
        private readonly List<Action> validationTasks = [];

        public required T Node { get; init; }

        public Logger<string> Log { get; } = new();

        public required Cache<string, HIR.Node> Scope { get; init; }

        // I also need to be able to:
        // AST.Expression -> object
        // Access the interpreter?

        // And to compile code we need Identifier -> CompilerObject ??? Why 2 scopes?
        // Maybe it's because we don't really need Identifier -> HIR.Node.
        // We need COs for CT values. These can be used in evaluation via Expose.

        // So the expression compiler is going to do AST -> HIR -> CO -> IR -> object, huh?
        // That is, for evaluation. For HIR access we do AST -> HIR.
        // COs are produces by the CO loader.
        // Not every AST has a corresponding HIR node not a metatype.
        // Some AST nodes are desugared.
        // In any case, we execute statements one by one.

        // In any case, we don't really care about things like CO or IR.
        // We want objects. We want slots. We want AST nodes.

        // Design thing: implement all compilation logic as static functions.
        // Or be ready to construct a new compiler per unique/scoped context.

        public DefinitionCompiler DefinitionCompiler { get; init; }

        public ExpressionCompiler ExpressionCompiler { get; init; }

        public void AddBuildTask(Action task)
            => buildTasks.Add(task);

        public void AddValidationTask(Action task)
            => validationTasks.Add(task);
    }
}
