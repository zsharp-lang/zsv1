using CommonZ.Utils;

namespace ZSharp.Compiler.Features.Callable
{
    public sealed class CallSite
    {
        public required CompilerObject Callable { get; init; }

        public Collection<BoundParameter> Arguments { get; init; } = [];
    }
}
