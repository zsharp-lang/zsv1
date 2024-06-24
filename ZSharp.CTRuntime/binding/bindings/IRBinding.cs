namespace ZSharp.CTRuntime
{
    public abstract class IRBinding<T>(T @object, ZSType type)
        where T : IR.IRObject
    {
        public virtual ZSType Type { get; internal set; } = type;

        public T IR { get; } = @object;
    }
}
