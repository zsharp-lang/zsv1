using CommonZ.Utils;

namespace ZSharp.Objects
{
    public interface IClass
    {
        public string? Name { get; }

        public IClass? Base { get; }

        //public Collection<InterfaceImplementation> InterfaceImplementations { get; }
    }
}
