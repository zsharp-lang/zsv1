namespace ZSharp.Objects
{
    public interface IImplementsSpecification
    {
        public virtual void OnImplementSpecification(Compiler.Compiler compiler, IAbstraction abstraction, CompilerObject specification) { }
    }
}
