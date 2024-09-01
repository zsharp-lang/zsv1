namespace ZSharp.IR.VM
{
    public abstract class PutValue<T>(T value)
        : Put
        , IHasOperand<T>
        where T : notnull
    {
        public T Value { get; set; } = value;

        T IHasOperand<T>.Operand => Value;
    }
}
