using CommonZ.Utils;

namespace ZSharp.IR
{
    public sealed class ConstructedMethod(Method method)
        : MethodReference(method)
        , ICallable
    {
        public Collection<IType> Arguments { get; } = 
            method.HasGenericParameters 
            ? [] 
            : throw new InvalidOperationException($"Can not create generic instance from a non-generic method");
    }
}
