using CommonZ.Utils;

namespace ZSharp.IR
{
    public sealed class ConstructedClass(Class @class)
        : ConstructedType<Class>
    {
        public Class Class { get; set; } = @class;

        Class TypeReference<Class>.Definition => Class;

        public TypeReference? OwningType { get; set; }

        public Collection<IType> Arguments { get; set; } = @class.HasGenericParameters ? [] : Collection<IType>.Empty;
    }
}
