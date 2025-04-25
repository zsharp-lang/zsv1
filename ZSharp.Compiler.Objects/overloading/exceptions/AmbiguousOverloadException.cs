using ZSharp.Compiler;

namespace ZSharp.Objects
{
    public sealed class AmbiguousOverloadException(
        CompilerObject group, 
        Argument[] arguments,
        CompilerObject[] overloads
    ) 
        : OverloadException(group, arguments)
    {
        public CompilerObject[] Overloads { get; } = overloads;
    }
}
