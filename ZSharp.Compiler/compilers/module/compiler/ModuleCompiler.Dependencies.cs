using CommonZ.Utils;
using ZSharp.RAST;

namespace ZSharp.Compiler
{
    internal sealed partial class ModuleCompiler
    {
        private readonly Cache<CGObject, NodeObject> nodes = new();
        private NodeObject? currentDependent = null;

        private void AddDependency(
            DependencyState dependentState,
            NodeObject dependency,
            DependencyState state = DependencyState.Declared
        )
            => objectBuilder.AddDependencies(
                currentDependent ?? throw new InvalidOperationException(),
                dependentState,
                new DependencyNode<NodeObject>(dependency, state)
            );

        private void AddDependenciesForDeclaration(
            CGObject dependency,
            DependencyState state = DependencyState.Defined
        )
        {
            if (nodes.Cache(dependency, out var nodeObject))
                AddDependenciesForDeclaration(nodeObject, state);
        }

        private void AddDependencyForDefinition(
            CGObject dependency,
            DependencyState state = DependencyState.Declared
        )
        {
            if (nodes.Cache(dependency, out var nodeObject))
                AddDependencyForDefinition(nodeObject, state);
        }

        private void AddDependenciesForDeclaration(
            NodeObject dependency,
            DependencyState state = DependencyState.Defined
        )
            => AddDependency(DependencyState.Declared, dependency, state);

        private void AddDependencyForDefinition(
            NodeObject dependency,
            DependencyState state = DependencyState.Declared
        )
            => AddDependency(DependencyState.Defined, dependency, state);

        public void AddDependencies(
            DependencyState dependentState, 
            DependencyState dependenciesState, 
            RExpression node
        )
            => AddDependencies(
                dependentState,
                dependenciesState,
                new RExpressionWalker<RName, RName>(name => name)
                .Walk(node)
                .Select(name => 
                    Context.CurrentScope.Cache(name.Name, out var result) &&
                    nodes.Cache(result, out var nodeObject) ? nodeObject : null
                )
                .Where(dependency => dependency is not null).ToArray()!
            );

        public void AddDependencies(
            DependencyState dependentState,
            DependencyState dependenciesState,
            RStatement node
        )
            => AddDependencies(
                dependentState,
                dependenciesState,
                new RExpressionWalker<RName, RName>(name => name)
                .Walk(node)
                .Select(name =>
                    Context.CurrentScope.Cache(name.Name, out var result) &&
                    nodes.Cache(result, out var nodeObject) ? nodeObject : null
                )
                .Where(dependency => dependency is not null).ToArray()!
            );

        private void AddDependencies(
            DependencyState dependentState,
            DependencyState dependenciesState,
            params NodeObject[] dependencies
        )
            => objectBuilder.AddDependencies(
                currentDependent ?? throw new InvalidOperationException(),
                dependentState,
                dependencies.Select(dependency => new DependencyNode<NodeObject>(dependency, dependenciesState)).ToArray()
            );

        private void AddDependenciesForDeclaration(
            DependencyState state = DependencyState.Defined,
            params NodeObject[] dependencies
        )
            => AddDependencies(DependencyState.Declared, state, dependencies);

        private void AddDependenciesForDefinition(
            DependencyState state = DependencyState.Declared,
            params NodeObject[] dependencies
        )
            => AddDependencies(DependencyState.Defined, state, dependencies);

        private void AddDependenciesForDeclaration(
            RExpression node,
            DependencyState state = DependencyState.Defined
        )
            => AddDependencies(DependencyState.Declared, state, node);

        private void AddDependenciesForDefinition(
            RExpression node,
            DependencyState state = DependencyState.Declared
        )
            => AddDependencies(DependencyState.Defined, state, node);

        private void AddDependenciesForDeclaration(
            RStatement node,
            DependencyState state = DependencyState.Defined
        )
    => AddDependencies(DependencyState.Declared, state, node);

        private void AddDependenciesForDefinition(
            RStatement node,
            DependencyState state = DependencyState.Declared
        )
            => AddDependencies(DependencyState.Defined, state, node);

        private ContextManager CollectingDependenciesFor(NodeObject compiler)
        {
            (currentDependent, compiler) = (compiler, currentDependent);

            return new(() =>
            {
                currentDependent = compiler;
            });
        }
    }
}
