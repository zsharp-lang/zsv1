using ZSharp.Compiler;

namespace ZSharp.Objects
{
    public abstract class OverloadException(OverloadGroup group, Argument[] arguments) 
        : ArgumentMismatchException(group, arguments)
    {
        public OverloadGroup Group { get; } = group;
    }
}
