namespace ZSharp.Compiler
{
    /// <summary>
    /// Defines the state of the dependency that the depndant requires.
    /// </summary>
    internal enum DependencyState
    {
        /// <summary>
        /// The dependency must be at least declared. Corresponds to <see cref="ObjectState.Declared"/>.
        /// </summary>
        Declared,

        /// <summary>
        /// The dependency must be fully defined. Corresponds to <see cref="ObjectState.Defined"/>.
        /// </summary>
        Defined,

        /// <summary>
        /// The dependency must be fully built. Corresponds to <see cref="ObjectState.Built"/>.
        /// </summary>
        Built = Defined,
    }
}
