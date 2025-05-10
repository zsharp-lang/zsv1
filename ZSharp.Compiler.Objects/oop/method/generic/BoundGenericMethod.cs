using ZSharp.Compiler;

namespace ZSharp.Objects
{
    public sealed class BoundGenericMethod(GenericMethod method, CompilerObject instance)
        : CompilerObject
        , ICTCallable
        , ICTGetIndex
        , IReferencable<BoundGenericMethodInstance>
    {
        public GenericMethod Method { get; } = method;

        public CompilerObject Instance { get; } = instance;

        public CompilerObject Call(Compiler.Compiler compiler, Argument[] arguments)
            => compiler.Call(Method, [new(Instance), .. arguments]);

        #region Index

        CompilerObject ICTGetIndex.Index(Compiler.Compiler compiler, Argument[] index)
        {
            var context = new ReferenceContext();

            foreach (var (genericParameter, genericArgument) in Method.GenericParameters.Zip(index))
                context[genericParameter] = genericArgument.Object;

            return compiler.Feature<Referencing>().CreateReference(this, context);
        }

        #endregion

        #region Reference

        BoundGenericMethodInstance IReferencable<BoundGenericMethodInstance>.CreateReference(Referencing @ref, ReferenceContext context)
            => new(
                @ref.CreateReference<GenericMethodInstance>(Method, context),
                Instance
            );

        #endregion
    }
}
