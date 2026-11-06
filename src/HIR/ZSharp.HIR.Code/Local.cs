namespace ZSharp.HIR.Code
{
    /// <summary>
    /// A variable of the block that declares it.
    /// </summary>
    public sealed class Local(string name, Type type)
        : Definition
        , Instance
    {
        public string Name { get; set; } = name;

        public Type Type { get; set; } = type;
    }
}
