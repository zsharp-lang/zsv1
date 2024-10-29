using ZSharp.CGObjects;

namespace ZSharp.Compiler
{
    public sealed partial class Context
    {
        public CGObject DefaultMetaClass { get; set; } = new ClassMeta();
    }
}
