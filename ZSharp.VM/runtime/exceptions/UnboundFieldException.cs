using ZSharp.IR;

namespace ZSharp.VM
{
    public sealed class UnboundFieldException(Field field) : ZSRuntimeException
    {
        public Field Field { get; } = field;
    }
}
