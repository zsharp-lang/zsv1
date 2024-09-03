using CommonZ.Utils;
using ZSharp.IR;

namespace ZSharp.VM
{
    public sealed partial class Runtime
    {
        internal readonly Cache<IRObject, ZSObject> irMap = [];
    }
}
