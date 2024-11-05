using CommonZ.Utils;

namespace ZSharp.Compiler
{
    public sealed partial class Compiler
    {
        public Cache<string, CGObject> Operators { get; } = new();
    }
}
