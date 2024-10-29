using CommonZ.Utils;
using ZSharp.RAST;

namespace ZSharp.Compiler
{
    internal sealed class NonLinearContent(
            Context context, 
            IObjectBuilder<NodeObject> objectBuilder,
            IObjectInitializer<NodeObject> objectInitializer
        )
    {
        private readonly Context context = context;
        private readonly ObjectBuilder<NodeObject> objectBuilder = new(objectBuilder);

        private NodeObject? currentDependent = null;

        private readonly Queue<NodeObject> dependencyCollectionQueue = [];

        public Cache<CGObject, NodeObject> Nodes { get; } = new();

        public void EnqueueForDependencyCollection(CGObject @object, RDefinition node)
            => EnqueueForDependencyCollection(new NodeObject(node, @object));

        public void EnqueueForDependencyCollection(NodeObject @object)
        {
            dependencyCollectionQueue.Enqueue(@object);
            Nodes.Cache(@object.Object, @object);
        }

        public void Build()
        {
            NodeObject @object;

            while (dependencyCollectionQueue.Count > 0)
                using (CollectingDependenciesFor(@object = dependencyCollectionQueue.Dequeue()))
                    objectInitializer.Initialize(@object);

            objectBuilder.BuildInOrder();
        }

        public void Clear()
        {
            objectBuilder.Clear();
            dependencyCollectionQueue.Clear();
            Nodes.Clear();
        }

        public void AddDependency(
            DependencyState dependentState,
            NodeObject dependency,
            DependencyState state = DependencyState.Declared
        )
            => objectBuilder.AddDependencies(
                currentDependent ?? throw new InvalidOperationException(),
                dependentState,
                new DependencyNode<NodeObject>(dependency, state)
            );

        public void AddDependenciesForDeclaration(
            CGObject dependency,
            DependencyState state = DependencyState.Defined
        )
        {
            if (Nodes.Cache(dependency, out var nodeObject))
                AddDependenciesForDeclaration(nodeObject, state);
        }

        public void AddDependencyForDefinition(
            CGObject dependency,
            DependencyState state = DependencyState.Declared
        )
        {
            if (Nodes.Cache(dependency, out var nodeObject))
                AddDependencyForDefinition(nodeObject, state);
        }

        public void AddDependenciesForDeclaration(
            NodeObject dependency,
            DependencyState state = DependencyState.Defined
        )
            => AddDependency(DependencyState.Declared, dependency, state);

        public void AddDependencyForDefinition(
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
                    context.CurrentScope.Cache(name.Name, out var result) &&
                    Nodes.Cache(result, out var nodeObject) ? nodeObject : null
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
                    context.CurrentScope.Cache(name.Name, out var result) &&
                    Nodes.Cache(result, out var nodeObject) ? nodeObject : null
                )
                .Where(dependency => dependency is not null).ToArray()!
            );

        public void AddDependencies(
            DependencyState dependentState,
            DependencyState dependenciesState,
            params NodeObject[] dependencies
        )
            => objectBuilder.AddDependencies(
                currentDependent ?? throw new InvalidOperationException(),
                dependentState,
                dependencies.Select(dependency => new DependencyNode<NodeObject>(dependency, dependenciesState)).ToArray()
            );

        public void AddDependenciesForDeclaration(
            DependencyState state = DependencyState.Defined,
            params NodeObject[] dependencies
        )
            => AddDependencies(DependencyState.Declared, state, dependencies);

        public void AddDependenciesForDefinition(
            DependencyState state = DependencyState.Declared,
            params NodeObject[] dependencies
        )
            => AddDependencies(DependencyState.Defined, state, dependencies);

        public void AddDependenciesForDeclaration(
            RExpression node,
            DependencyState state = DependencyState.Defined
        )
            => AddDependencies(DependencyState.Declared, state, node);

        public void AddDependenciesForDefinition(
            RExpression node,
            DependencyState state = DependencyState.Declared
        )
            => AddDependencies(DependencyState.Defined, state, node);

        public void AddDependenciesForDeclaration(
            RStatement node,
            DependencyState state = DependencyState.Defined
        )
            => AddDependencies(DependencyState.Declared, state, node);

        public void AddDependenciesForDefinition(
            RStatement node,
            DependencyState state = DependencyState.Declared
        )
            => AddDependencies(DependencyState.Defined, state, node);

        public ContextManager CollectingDependenciesFor(NodeObject compiler)
        {
            (currentDependent, compiler) = (compiler, currentDependent);

            return new(() =>
            {
                currentDependent = compiler;
            });
        }
    }
}
