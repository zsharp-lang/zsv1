namespace ZSharp.Compiler
{
    public interface ITypeAssignableToType
        : IType
    {
        public bool? IsAssignableTo(Compiler compiler, IType target)
            => null;
    }

    public interface ITypeAssignableFromType
        : IType
    {
        public bool? IsAssignableFrom(Compiler compiler, IType source)
            => null;
    }
}
