namespace ZSharp.Compiler
{
    /// <summary>
    /// Defines the state of an object that is being processed.
    /// This can be used to detect whther an object depending on the dependency can be
    /// processed or not.
    /// </summary>
    internal enum ObjectState
    {
        /// <summary>
        /// The object was not processed yet.
        /// </summary>
        Uninitialized,

        /// <summary>
        /// The object is currently collecting dependencies for the declaration process.
        /// </summary>
        Initializing,
        
        /// <summary>
        /// The object finsihed collecting dependencies for the declaration process and is now
        /// ready to be declard.
        /// </summary>
        Initialized,

        /// <summary>
        /// The object is current being declared.
        /// </summary>
        Declaring,

        /// <summary>
        /// The object is declared and can be used for referencing.
        /// </summary>
        Declared,

        /// <summary>
        /// The object is currently being defined.
        /// </summary>
        Defining,

        /// <summary>
        /// The object is now fully defined and can be used.
        /// </summary>
        Defined,

        /// <summary>
        /// Alias for <see cref="Defined"/>.
        /// </summary>
        Built = Defined,
    }
}
