using ZSharp.Compiler;

namespace ZSharp.Objects
{
    public sealed class NoOverloadFoundException(CompilerObject group, Argument[] arguments)
        : OverloadException(group, arguments)
    {
    }
}
