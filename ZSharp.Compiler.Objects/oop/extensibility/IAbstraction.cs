namespace ZSharp.Objects
{
    public interface IAbstraction
    {
        public virtual void OnDerivation(CompilerObject derived) { }

        public virtual void OnImplementation(CompilerObject implementor) { }
    }
}
