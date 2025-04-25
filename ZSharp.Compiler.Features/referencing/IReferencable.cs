namespace ZSharp.Objects
{
    public interface IReferencable : CompilerObject
    {
        public CompilerObject CreateReference(Compiler.Referencing @ref, ReferenceContext context);
    }

    public interface IReferencable<out T> : IReferencable
        where T : CompilerObject
    {
        CompilerObject IReferencable.CreateReference(Compiler.Referencing @ref, ReferenceContext context)
            => CreateReference(@ref, context);

        public new T CreateReference(Compiler.Referencing @ref, ReferenceContext context);
    }
}
