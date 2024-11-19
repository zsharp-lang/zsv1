using ZSharp.Compiler;

namespace ZSharp.CGObjects
{
    public interface IRTBoundMember
    {
        public CGObject Bind(Compiler.Compiler compiler, CGObject value);
    }
}
