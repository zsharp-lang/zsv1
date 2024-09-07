using CommonZ.Utils;
using ZSharp.Compiler;

namespace ZSharp.CGObjects
{
    public sealed class Document : CGObject
    {
        public IR.Module IR { get; set; } = new(null);

        public Collection<CGObject> Content { get; init; } = [];
    }
}
