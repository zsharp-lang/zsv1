using CommonZ.Utils;

namespace ZSharp.Compiler
{
    public sealed class InferenceTarget(CompilerObject target)
    {
        public CompilerObject Target { get; } = target;

        public Collection<Constraint> Constraints { get; } = new();

        public InferenceTargetResolver? Resolve { get; set; }
    }
}
