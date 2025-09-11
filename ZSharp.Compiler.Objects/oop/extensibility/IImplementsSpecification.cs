namespace ZSharp.Objects
{
    public interface IImplementsSpecification
        : CompilerObject
    {
        public virtual void OnImplementSpecification(Compiler.Compiler compiler, IAbstraction abstraction, CompilerObject specification) { }
    }
}
