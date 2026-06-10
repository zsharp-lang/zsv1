using ZSharp.HIR;

namespace Package.DotNet
{
    public sealed partial class Parameter
    {
        public required string Name { get; set; }

        public required ZSharp.HIR.Type Type { get; set; }

        public Expression? DefaultValue { get; set; }  // todo: only allow primitive values
    }
}
