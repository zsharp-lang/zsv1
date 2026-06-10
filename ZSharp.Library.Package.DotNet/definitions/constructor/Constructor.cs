using ZSharp.HIR;

namespace Package.DotNet
{
    public sealed partial class Constructor
        : Definition
    {
        public List<Parameter> Parameters { get; init; } = [];

        public Code Body { get; set; } = new();
    }
}
