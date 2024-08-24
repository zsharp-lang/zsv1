using CommonZ.Utils;

namespace ZSharp.Compiler
{
    internal struct Dependencies<T>(T dependant)
    {
        private readonly T dependant = dependant;

        private Collection<DependencyNode<T>>? declarationDependencies = null;
        private Collection<DependencyNode<T>>? definitionDependencies = null;

        public readonly T Dependant => dependant;

        public Collection<DependencyNode<T>> DeclarationDependency
        {
            get
            {
                if (declarationDependencies is not null)
                    return declarationDependencies;

                Interlocked.CompareExchange(ref declarationDependencies, [], null);
                return declarationDependencies;
            }
        }

        public Collection<DependencyNode<T>> DefinitionDependencies
        {
            get
            {
                if (definitionDependencies is not null)
                    return definitionDependencies;

                Interlocked.CompareExchange(ref definitionDependencies, [], null);
                return definitionDependencies;
            }
        }

        public readonly bool HasDeclarationDependencies => (declarationDependencies?.Count ?? 0) > 0;

        public readonly bool HasDefinitionDependencies => (definitionDependencies?.Count ?? 0) > 0;

        /// <summary>
        /// Adds dependencies required for this object to be declared.
        /// </summary>
        /// <param name="state">The state of the dependencies needed for the declaration of this object.</param>
        /// <param name="dependencies">The dependencies that this object depened on.</param>
        public void AddDeclarationDependencies(DependencyState state, params T[] dependencies)
            => DeclarationDependency.AddRange(
                dependencies.Select(
                    dependency => new DependencyNode<T>(dependency, state)
                )
            );

        /// <summary>
        /// Adds dependencies required for this object to be defined.
        /// </summary>
        /// <param name="state">The state of the dependencies needed for the definition of this object.</param>
        /// <param name="dependencies">The dependencies that this object depened on.</param>
        public void AddDefinitionDependencies(DependencyState state, params T[] dependencies)
            => DefinitionDependencies.AddRange(
                dependencies.Select(
                    dependency => new DependencyNode<T>(dependency, state)
                )
            );
    }
}
