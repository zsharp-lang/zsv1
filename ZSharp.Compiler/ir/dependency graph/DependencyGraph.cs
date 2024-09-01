using CommonZ.Utils;

namespace ZSharp.Compiler
{
    internal sealed partial class DependencyGraph<T>
        where T : class
    {
        private readonly Mapping<DependencyNode<T>, Collection<DependencyNode<T>>> graph = [];

        public void AddDependenciesForDeclarationOf(T dependent, params DependencyNode<T>[] dependencies)
            => AddDependencies(dependent, DependencyState.Declared, dependencies);

        public void AddDependenciesForDefinitionOf(T dependent, params DependencyNode<T>[] dependencies)
            => AddDependencies(dependent, DependencyState.Defined, dependencies);

        public void AddDependencies(T dependent, DependencyState state, params DependencyNode<T>[] dependencies)
            => AddDependencies(new(dependent, state), dependencies);

        public void AddDependencies(DependencyNode<T> dependent, params DependencyNode<T>[] dependencyNodes)
        {
            if (!graph.TryGetValue(dependent, out var dependencies))
                graph[dependent] = dependencies = [];

            dependencies.AddRange(dependencyNodes);
        }
    }
}
