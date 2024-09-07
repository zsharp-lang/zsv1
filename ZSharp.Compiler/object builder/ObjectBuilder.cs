namespace ZSharp.Compiler
{
    internal sealed class ObjectBuilder<T>(IObjectBuilder<T> objectBuilder)
        where T : class
    {
        private readonly IObjectBuilder<T> objectBuilder = objectBuilder;
        private readonly DependencyGraph<T> dependencyGraph = [];

        public void AddDependenciesForDeclarationOf(T dependent, params DependencyNode<T>[] dependencies)
            => dependencyGraph.AddDependenciesForDeclarationOf(dependent, dependencies);

        public void AddDependenciesForDefinitionOf(T dependent, params DependencyNode<T>[] dependencies)
            => dependencyGraph.AddDependenciesForDefinitionOf(dependent, dependencies);

        public void AddDependencies(T dependent, DependencyState state, params DependencyNode<T>[] dependencies)
            => dependencyGraph.AddDependencies(dependent, state, dependencies);

        public void AddDependencies(DependencyNode<T> dependent, params DependencyNode<T>[] dependencies)
            => dependencyGraph.AddDependencies(dependent, dependencies);

        public void BuildInOrder()
        {
            foreach (var objects in dependencyGraph)
            foreach (var @object in objects)
                BuildSingle(@object);
        }

        private void BuildSingle(DependencyNode<T> @object)
        {
            switch (@object.State)
            {
                case DependencyState.Declared: BuildDeclaration(@object.Dependency); break;
                case DependencyState.Defined: BuildDefinition(@object.Dependency); break;
                default: throw new($"Invalid state {@object.State}");
            }
        }

        private void BuildDeclaration(T @object)
            => objectBuilder.Declare(@object);

        private void BuildDefinition(T @object)
            => objectBuilder.Define(@object);
    }
}
