using CommonZ.Utils;

namespace ZSharp.IR
{
    public interface ConstructedType
        : OOPTypeReference
    {
        public abstract Collection<IType> Arguments { get; }
    }

    public interface ConstructedType<T> 
        : ConstructedType
        , OOPTypeReference<T>
        where T : OOPType
    {
        
    }
}
