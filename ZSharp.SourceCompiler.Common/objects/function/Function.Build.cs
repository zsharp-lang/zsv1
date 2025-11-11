using System.Diagnostics.CodeAnalysis;

namespace ZSharp.SourceCompiler.Objects
{
    partial class Function
    {
        [Flags]
        private enum BuildState
        {
            Owner = 1 << 0,
            Signature = 1 << 1,
            Body = 1 << 2,
        }

        private readonly ObjectBuildState<BuildState> state = new();

        [MemberNotNullWhen(true, nameof(IR))]
        public bool IsBuilt => IR is not null;
    }
}
