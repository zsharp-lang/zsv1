namespace ZSharp.Compiler
{
    internal struct DependencyNode<T>(T dependency, DependencyState state)
    {
        public T Dependency { get; } = dependency;

        public DependencyState State { get; set; } = state;
    }
}
