namespace Package.DotNet
{
    public sealed class Field(string name, ZSharp.HIR.Type type)
        : ZSharp.HIR.Definition
        , ZSharp.HIR.Instance
    {
        public string Name { get; set; } = name;

        public ZSharp.HIR.Type Type { get; set; } = type;

        public bool IsStatic { get; set; }

        public ZSharp.HIR.Code.Expression? DefaultValue { get; set; }
    }
}
