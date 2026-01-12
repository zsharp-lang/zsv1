using CommonZ.Utils;

namespace ZSharp.IR
{
    public sealed class InterfaceImplementation(TypeReference<Interface> @interface)
    {
        public TypeReference<Interface> Interface { get; set; } = @interface;

        /// <summary>
        /// Mapping from interface method to implementation method.
        /// </summary>
        public Mapping<MethodReference, Method> Implementations { get; } = [];
    }
}
