using ZSharp.Compiler;

namespace ZSharp.Objects
{
    public sealed class NoOverloadFoundException(OverloadGroup group, Argument[] arguments)
        : OverloadException(group, arguments)
    {
    }
}
