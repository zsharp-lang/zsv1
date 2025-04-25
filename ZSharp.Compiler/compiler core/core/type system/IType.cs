namespace ZSharp.Compiler
{
    public interface IType
        : CompilerObject
    {
        public bool IsEqualTo(Compiler compiler, IType type)
            => ReferenceEquals(this, type);
    }
}
