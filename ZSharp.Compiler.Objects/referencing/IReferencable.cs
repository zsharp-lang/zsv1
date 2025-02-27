namespace ZSharp.Objects
{
    public interface IReferencable
    {
        public CompilerObject CreateReference(Compiler.Compiler compiler, ReferenceContext context);
    }
}
