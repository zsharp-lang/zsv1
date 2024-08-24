using CommonZ.Utils;

namespace ZSharp.Compiler
{
    internal sealed partial class DependencyGraph<T>
        where T : notnull
    {
        private readonly Mapping<T, ObjectState> states = [];
        private readonly Mapping<T, Collection<DependencyNode<T>>> graph = [];
        private readonly Collection<Collection<T>> order = [];

        public void Add(ObjectState type, T dependant, params T[] dependencies)
        {
            Add(dependant);

            var deps = graph[dependant];

            foreach (var dependency in dependencies)
                deps.Add(new(dependency, type));
        }

        public void Add(T dependant)
        {
            if (!states.ContainsKey(dependant))
                states[dependant] = ObjectState.Uninitialized;

            if (graph.ContainsKey(dependant))
                graph[dependant] = [];
        }

        public void SetState(T dependant, ObjectState state)
            => states[dependant] = state;

        private void CalculateOrder()
        {
            Mapping<int, Collection<T>> dependencyLevels = [];

            foreach (var (dependant, dependencies) in graph)
            {
                foreach (var dependency in dependencies)
                {
                    if (!IsDependencyMet(dependency))

                }
            }
        }

        private bool IsDependencyMet(DependencyNode<T> dependency)
        {
            if (!states.TryGetValue(dependency.Dependency, out var state))
                return false;

            return state >= dependency.State;
        }
    }
}
