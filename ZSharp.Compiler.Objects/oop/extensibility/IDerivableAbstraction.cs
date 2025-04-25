namespace ZSharp.Objects
{
    public interface IDerivableAbstraction
    {
        public virtual void OnDerivation(CompilerObject derived) { }

        public virtual void OnImplementation(CompilerObject implementor) { }
    }
}
