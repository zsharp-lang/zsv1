using CommonZ.Utils;

namespace ZSharp.Compiler
{
    readonly struct CurrentlyBuiltObject
    {
        public static readonly CurrentlyBuiltObject Empty = new()
        {
            Object = null!,
            State = ObjectState.Uninitialized
        };

        public required CGObject Object { get; init; }

        public required ObjectState State { get; init; }
    }

    internal partial class IRGenerator
    {
        private readonly DependencyGraph<CGObject> dependencyGraph = [];
        private CurrentlyBuiltObject currentlyBuiltObject = ZSharp.Compiler.CurrentlyBuiltObject.Empty;

        public bool HasCurrentlyBuiltObject => currentlyBuiltObject.Object is not null;

        public CGObject CurrentlyBuiltObject => currentlyBuiltObject.Object;

        public ObjectState CurrentlyBuiltObjectState => currentlyBuiltObject.State;

        public void RegisterObject(CGObject cgObject)
            => dependencyGraph.Add(cgObject);

        public void AddDependencies(ObjectState state, params CGObject[] dependencies)
            => dependencyGraph.Add(state, CurrentlyBuiltObject, dependencies);

        public void AddDependencies(ObjectState state, CGCode code)
        {
            foreach (var item in code)
            {

            }
        }

        public bool IsInitialized(CGObject @object)
            => GetObjectState(@object) == ObjectState.Initialized;

        public bool IsDeclared(CGObject @object)
            => GetObjectState(@object) == ObjectState.Declared;

        public bool IsDefined(CGObject @object)
            => GetObjectState(@object) == ObjectState.Defined;

        public bool IsBuilt(CGObject @object)
            => GetObjectState(@object) == ObjectState.Built;

        /// <summary>
        /// Uses the given object as the currently built object with the state <see cref="ObjectState.Initializing"/>.
        /// 
        /// While in this state, dependencies are added as dependencies for the declaration of the object.
        /// </summary>
        /// <param name="object">The object to be set as the currently built object.</param>
        /// <returns>A context manager to control the lifetime of this object being the currently
        /// built object.</returns>
        private ContextManager Initializing(CGObject @object)
            => SetCurrentlyBuiltObject(@object, ObjectState.Initializing);

        /// <summary>
        /// Uses the given object as the currently built object with the state <see cref="ObjectState.Declaring"/>.
        /// 
        /// While in this state, dependencies are added as dependencies for the definition of the object.
        /// </summary>
        /// <param name="object">The object to be set as the currently built object.</param>
        /// <returns>A context manager to control the lifetime of this object being the currently
        /// built object.</returns>
        private ContextManager Declaring(CGObject @object)
            => SetCurrentlyBuiltObject(@object, ObjectState.Declaring);

        /// <summary>
        /// Uses the given object as the currently built object with the state <see cref="ObjectState.Defining"/>.
        /// 
        /// While in this state, dependencies cannot be added as this is the final state of the object.
        /// </summary>
        /// <param name="object">The object to be set as the currently built object.</param>
        /// <returns>A context manager to control the lifetime of this object being the currently
        /// built object.</returns>
        private ContextManager Defining(CGObject @object)
            => SetCurrentlyBuiltObject(@object, ObjectState.Defining);

        private ObjectState GetObjectState(CGObject @object)
            => dependencyGraph.GetObjectState(@object);

        private ContextManager SetCurrentlyBuiltObject(CGObject @object, ObjectState state)
        {
            CurrentlyBuiltObject builtObject = new()
            {
                Object = @object,
                State = state
            };

            (currentlyBuiltObject, builtObject) = (builtObject, currentlyBuiltObject);

            return new(() =>
            {
                currentlyBuiltObject = builtObject;
            });
        }
	}
}
