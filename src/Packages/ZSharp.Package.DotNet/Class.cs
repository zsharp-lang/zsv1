namespace Package.DotNet
{
    public sealed class Class(string name)
        : ZSharp.HIR.Definition
        , ZSharp.HIR.Instance
        , ZSharp.HIR.Type
    {
        public string Name { get; set; } = name;
    }
}
