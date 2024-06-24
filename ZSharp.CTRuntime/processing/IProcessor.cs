using ZSharp.RAST;

namespace ZSharp.CTRuntime
{
    internal interface IProcessor<T>
    {
        IBinding<T> Assign(IBinding<T> target, IBinding<T> value);

        IBinding<T> Call(IBinding<T> callee, Argument<T>[] arguments);

        IBinding<T> Cast(IBinding<T> instance, IBinding<T> targetType);

        IBinding<T> Literal(object value, RLiteralType literalType, IBinding<T>? unitType);
    }
}
