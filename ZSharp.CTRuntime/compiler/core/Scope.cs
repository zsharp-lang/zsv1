using CommonZ.Utils;
using ZSharp.RAST;

namespace ZSharp.CTRuntime
{
    public sealed class Scope<T> : Cache<RNode, IBinding<T>>
    {
    }
}
