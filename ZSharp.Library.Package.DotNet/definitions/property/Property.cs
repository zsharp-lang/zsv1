using ZSharp.HIR;

namespace Package.DotNet
{
    public sealed partial class Property
        : Definition
    {
        public required string Name { get; set; }

        public required ZSharp.HIR.Type Type { get; set; }

        public Method? Getter { get; set; }

        public Method? Setter { get; set; }
    }
}
