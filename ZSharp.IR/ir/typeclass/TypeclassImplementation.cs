using CommonZ.Utils;

namespace ZSharp.IR
{
    public sealed class TypeclassImplementation(Typeclass typeclass) : TypeDefinition
    {
        public Typeclass Typeclass { get; } = typeclass;

        public Mapping<Method, Method> Implementations { get; }

        public Collection<Method> Methods { get; }
    }
}
