namespace ZSharp.IR.VM
{
    public interface IHasOperand
    {
        public object Operand { get; }
    }

    public interface IHasOperand<out T> : IHasOperand
        where T : notnull
    {
        object IHasOperand.Operand => Operand;

        public new T Operand { get; }
    }
}
