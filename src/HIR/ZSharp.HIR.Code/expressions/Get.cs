namespace ZSharp.HIR.Code
{
    public sealed class Get(Instance instance)
        : Expression
    {
        public Instance Instance { get; } = instance;
    }
}
