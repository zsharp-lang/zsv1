using ZSharp.RAST;

namespace ZSharp.CTRuntime
{
    public interface IBinder<T>
    {
        public IBinding<T> Bind(RExpression expression);
    }
}
