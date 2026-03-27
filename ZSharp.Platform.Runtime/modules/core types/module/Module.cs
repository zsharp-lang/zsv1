namespace ZSharp.Platform.Runtime.Modules.CoreTypes
{
    public sealed partial class Module
    {
        public IR.Module Definition { get; init; } = new(string.Empty);

        internal Module()
        {
            Definition.Types.Add((Bool = new(new("Boolean"))).Definition);
            Definition.Types.Add((Size = new(new("Size"))).Definition);
            Definition.Types.Add((Void = new(new("Void"))).Definition);

            Definition.Types.Add((Array = new(this, new("Array"))).Definition);
        }
    }
}
