using CommonZ.Utils;

namespace ZSharp.Compiler
{
    internal sealed partial class ModuleGenerator
    {
        private CGObject? currentDependent = null;

        private void AddDependency(
            DependencyState dependentState,
            CGObject dependency,
            DependencyState state = DependencyState.Declared
        )
            => objectBuilder.AddDependencies(
                currentDependent ?? throw new InvalidOperationException(),
                dependentState,
                new DependencyNode<CGObject>(dependency, state)
            );

        private void AddDependenciesForDeclaration(
            CGObject dependency,
            DependencyState state = DependencyState.Defined
        )
            => AddDependency(DependencyState.Declared, dependency, state);

        private void AddDependencyForDefinition(
            CGObject dependency,
            DependencyState state = DependencyState.Declared
        )
            => AddDependency(DependencyState.Defined, dependency, state);

        /// <summary>
        /// Add all objects that the given code directly depends on.
        /// 
        /// This only adds dependencies for objects that are directly accessed by the code.
        /// This includes the <see cref="CGRuntime.CG.Get(string)"/> and 
        /// <see cref="CGRuntime.CG.Object(CGObject)"/> instructions.
        /// </summary>
        /// <param name="dependentState">The state of the dependent.</param>
        /// <param name="dependenciesState">The state in which the found dependencies need to be.</param>
        /// <param name="code">The code to find dependencies in.</param>
        public void AddDependencies(
            DependencyState dependentState, 
            DependencyState dependenciesState, 
            CGCode code
        )
            => AddDependencies(
                dependentState,
                dependenciesState,
                code
                .Select(item => item switch
                {
                    CGRuntime.HLVM.Object @objectInstruction => objectInstruction.CGObject,
                    CGRuntime.HLVM.Binding bindingInstruction
                        when bindingInstruction.AccessMode == CGRuntime.HLVM.AccessMode.Get
                        => IRGenerator.CurrentScope.Cache(bindingInstruction.Name),
                    _ => null
                })
                .Where(dependency => dependency is not null)
                .ToArray()!
            );

        private void AddDependencies(
            DependencyState dependentState,
            DependencyState dependenciesState,
            params CGObject[] dependencies
        )
            => objectBuilder.AddDependencies(
                currentDependent ?? throw new InvalidOperationException(),
                dependentState,
                dependencies.Select(dependency => new DependencyNode<CGObject>(dependency, dependenciesState)).ToArray()
            );

        private void AddDependenciesForDeclaration(
            DependencyState state = DependencyState.Defined,
            params CGObject[] dependencies
        )
            => AddDependencies(DependencyState.Declared, state, dependencies);

        private void AddDependenciesForDefinition(
            DependencyState state = DependencyState.Declared,
            params CGObject[] dependencies
        )
            => AddDependencies(DependencyState.Defined, state, dependencies);

        private void AddDependenciesForDeclaration(
            CGCode code,
            DependencyState state = DependencyState.Defined
        )
            => AddDependencies(DependencyState.Declared, state, code);

        private void AddDependenciesForDefinition(
            CGCode code,
            DependencyState state = DependencyState.Declared
        )
            => AddDependencies(DependencyState.Defined, state, code);

        private ContextManager CollectingDependenciesFor(CGObject dependent)
        {
            (currentDependent, dependent) = (dependent, currentDependent);

            return new(() =>
            {
                currentDependent = dependent;
            });
        }
    }
}
