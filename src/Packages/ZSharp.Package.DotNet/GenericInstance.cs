namespace Package.DotNet
{
    /// <summary>
    /// A generic type given its arguments, as <c>List[Int32]</c>. An argument is any
    /// type, as a Z# class.
    /// </summary>
    public sealed class GenericInstance(TypeReference definition)
        : ClassReference
    {
        public TypeReference Definition { get; set; } = definition;

        public List<ZSharp.HIR.Type> Arguments { get; } = [];

        /// <summary>
        /// The instance of a definition given types, as code that runs builds it.
        /// </summary>
        public static GenericInstance Of(TypeReference definition, params ZSharp.HIR.Type[] arguments)
        {
            var instance = new GenericInstance(definition);
            instance.Arguments.AddRange(arguments);
            return instance;
        }
    }
}
