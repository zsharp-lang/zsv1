using CommonZ.Utils;

namespace ZSharp.IR
{
    public sealed class ConstructedClass(Class @class)
        : ConstructedType<Class>
    {
        public Class Class { get; set; } = @class;

        Class OOPTypeReference<Class>.Definition => Class;

        public OOPTypeReference? OwningType { get; set; }

        public Collection<IType> Arguments { get; set; } = @class.HasGenericParameters ? [] : Collection<IType>.Empty;
    }
}
