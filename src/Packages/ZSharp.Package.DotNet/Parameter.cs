namespace Package.DotNet
{
    public sealed class Parameter(string name, ZSharp.HIR.Type? type)
        : ZSharp.HIR.Instance
    {
        /// <summary>
        /// A parameter whose type is left to a rule of what it belongs to.
        /// </summary>
        public Parameter(string name)
            : this(name, null)
        {
        }

        public string Name { get; set; } = name;

        public ZSharp.HIR.Type? Type { get; set; } = type;

        public ZSharp.HIR.Code.Expression? DefaultValue { get; set; }
    }
}
