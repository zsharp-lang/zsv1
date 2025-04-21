using ZSharp.Objects;

namespace ZSharp.Compiler
{
    public sealed class Referencing(Compiler compiler)
        : Feature(compiler)
    {
        public CompilerObject CreateReference(CompilerObject @object, ReferenceContext context)
        {
            if (@object is IReferencable referencable)
                return referencable.CreateReference(this, context);

            return @object;
        }

        public T CreateReference<T>(CompilerObject @object, ReferenceContext context)
            where T : CompilerObject
        {
            if (@object is IReferencable<T> referencable)
                return referencable.CreateReference(this, context);

            throw new NotImplementedException();
        }
    }
}
