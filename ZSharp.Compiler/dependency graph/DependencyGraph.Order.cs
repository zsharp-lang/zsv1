using CommonZ.Utils;

namespace ZSharp.Compiler
{
    internal sealed partial class DependencyGraph<T>
        where T : class
    {
        private Collection<Collection<DependencyNode<T>>>? order = null;

        private enum DependencyNodeState
        {
            NotVisited,
            Visiting,
            Visited
        }

        internal Collection<Collection<DependencyNode<T>>> CalculateOrder()
        {
            Collection<Collection<DependencyNode<T>>> objectOrder = [];

            Mapping<DependencyNode<T>, int> objectLevels = [];
            Mapping<DependencyNode<T>, DependencyNodeState> nodeStates = [];

            int CalculateDependencyLevel(DependencyNode<T> @object)
            {
                if (!graph.TryGetValue(@object, out var dependencies))
                    return -1;

                if (objectLevels.TryGetValue(@object, out var level))
                    return level;

                if (!nodeStates.TryGetValue(@object, out var nodeState) || nodeState == DependencyNodeState.NotVisited)
                {
                    nodeStates[@object] = DependencyNodeState.Visiting;

                    int objectLevel;
                    objectLevel = objectLevels[@object] =
                        dependencies
                        .Select(CalculateDependencyLevel)
                        .Concat([-1])
                        .Max() + 1;
                    nodeStates[@object] = DependencyNodeState.Visited;
                    return objectLevel;
                }

                if (nodeState == DependencyNodeState.Visited)
                    return objectLevels[@object];

                if (nodeState == DependencyNodeState.Visiting)
                    throw new Exception("Circular dependency detected.");

                throw new Exception("Invalid state.");
            }

            foreach (var dependent in graph.Keys)
            {
                int level = CalculateDependencyLevel(dependent);
                while (level >= objectOrder.Count)
                    objectOrder.Add([]);
                objectOrder[level].Add(dependent);
            }

            return order = objectOrder;
        }
    }
}
