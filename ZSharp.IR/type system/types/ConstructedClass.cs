using CommonZ.Utils;

namespace ZSharp.IR
{
    public sealed class ConstructedClass(Class @class)
        : IType
    {
        public Class Class { get; set; } = @class;

        public Collection<IType> Arguments { get; set; } = @class.HasGenericParameters ? [] : Collection<IType>.Empty;
    }
}
