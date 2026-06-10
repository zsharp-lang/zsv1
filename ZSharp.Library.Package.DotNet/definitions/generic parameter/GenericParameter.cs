namespace Package.DotNet
{
    public sealed partial class GenericParameter
        : ZSharp.HIR.Type
    {
        public required string Name { get; set; }

        // todo: constraints
    }
}
