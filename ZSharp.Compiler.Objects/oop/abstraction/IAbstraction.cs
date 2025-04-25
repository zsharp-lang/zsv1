using CommonZ.Utils;

namespace ZSharp.Objects
{
    public interface IAbstraction
        : CompilerObject
    {
        public Collection<CompilerObject> Specifications { get; }
    }
}
