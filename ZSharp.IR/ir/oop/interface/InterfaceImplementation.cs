using CommonZ.Utils;

namespace ZSharp.IR
{
    public sealed class InterfaceImplementation(Interface @interface)
    {
        public Interface Interface { get; set; } = @interface;

        /// <summary>
        /// Mapping from interface method to implementation method.
        /// </summary>
        public Mapping<Method, Method> Implementations { get; } = [];
    }
}
