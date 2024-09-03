namespace ZSharp.Compiler
{
    internal sealed class ObjectBuilder(IObjectBuilder objectBuilder)
    {
        private readonly IObjectBuilder objectBuilder = objectBuilder;
        private readonly DependencyGraph<CGObject> dependencyGraph = [];

        public void AddDependenciesForDeclarationOf(CGObject dependent, params DependencyNode<CGObject>[] dependencies)
            => dependencyGraph.AddDependenciesForDeclarationOf(dependent, dependencies);

        public void AddDependenciesForDefinitionOf(CGObject dependent, params DependencyNode<CGObject>[] dependencies)
            => dependencyGraph.AddDependenciesForDefinitionOf(dependent, dependencies);

        public void AddDependencies(CGObject dependent, DependencyState state, params DependencyNode<CGObject>[] dependencies)
            => dependencyGraph.AddDependencies(dependent, state, dependencies);

        public void AddDependencies(DependencyNode<CGObject> dependent, params DependencyNode<CGObject>[] dependencies)
            => dependencyGraph.AddDependencies(dependent, dependencies);

        public void BuildInOrder()
        {
            foreach (var objects in dependencyGraph)
            foreach (var @object in objects)
                BuildSingle(@object);
        }

        private void BuildSingle(DependencyNode<CGObject> @object)
        {
            switch (@object.State)
            {
                case DependencyState.Declared: BuildDeclaration(@object.Dependency); break;
                case DependencyState.Defined: BuildDefinition(@object.Dependency); break;
                default: throw new($"Invalid state {@object.State}");
            }
        }

        private void BuildDeclaration(CGObject @object)
            => objectBuilder.Declare(@object);

        private void BuildDefinition(CGObject @object)
            => objectBuilder.Define(@object);
    }
}
