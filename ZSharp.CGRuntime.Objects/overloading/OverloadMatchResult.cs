using CommonZ.Utils;
using ZSharp.Compiler;

namespace ZSharp.CGObjects
{
    public sealed class OverloadMatchResult
    {
        public Collection<(Argument, Assignment)> Arguments { get; } = [];
    }
}
