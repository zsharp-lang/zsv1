namespace Package.DotNet
{
    public sealed class Parameter(string name, ZSharp.HIR.Type type)
        : ZSharp.HIR.Instance
    {
        public string Name { get; set; } = name;

        public ZSharp.HIR.Type Type { get; set; } = type;

        public ZSharp.HIR.Code.Expression? DefaultValue { get; set; }
    }
}
