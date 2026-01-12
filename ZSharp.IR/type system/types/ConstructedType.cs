using CommonZ.Utils;

namespace ZSharp.IR
{
    public interface ConstructedType
        : TypeReference
    {
        public abstract Collection<IType> Arguments { get; }
    }

    public interface ConstructedType<T> 
        : ConstructedType
        , TypeReference<T>
        where T : TypeDefinition
    {
        
    }
}
