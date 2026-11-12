namespace Package.DotNet
{
    /// <summary>
    /// A parameter as C# has them: an argument with its name fills it, or else the next
    /// positional one, or else its default value.
    /// </summary>
    public sealed class Parameter(string name, ZSharp.HIR.Type? type)
        : ZSharp.HIR.Code.Parameter
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
