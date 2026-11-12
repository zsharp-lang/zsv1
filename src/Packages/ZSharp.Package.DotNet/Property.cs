namespace Package.DotNet
{
    public sealed class Property(string name, ZSharp.HIR.Type? type)
        : ZSharp.HIR.Definition
        , ZSharp.HIR.Instance
        , ZSharp.HIR.Named
    {
        public string Name { get; set; } = name;

        /// <summary>
        /// Its type, as its accessors have it, or <see langword="null"/> if they don't.
        /// </summary>
        public ZSharp.HIR.Type? Type { get; set; } = type;

        public bool IsStatic { get; set; }

        public Method? Getter { get; set; }

        public Method? Setter { get; set; }
    }
}
