using System.Diagnostics.CodeAnalysis;

namespace ZSharp.Compiler
{
    internal struct DependencyNode<T>(T dependency, DependencyState state)
        where T : class
    {
        public T Dependency { get; } = dependency;

        public DependencyState State { get; set; } = state;

        public readonly override bool Equals([NotNullWhen(true)] object? obj)
        {
            if (obj is DependencyNode<T> node)
                return Equals(node);
            return base.Equals(obj);
        }

        public readonly override int GetHashCode()
            => (Dependency, State).GetHashCode();

        public readonly bool Equals(DependencyNode<T> other)
            => State == other.State && Dependency == other.Dependency;
    }
}
