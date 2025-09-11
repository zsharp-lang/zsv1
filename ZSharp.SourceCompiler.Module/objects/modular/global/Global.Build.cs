using System.Diagnostics.CodeAnalysis;

namespace ZSharp.SourceCompiler.Module.Objects
{
    partial class Global
    {
        [Flags]
        private enum BuildState
        {
            Owner = 1 << 0,

        }

        private readonly ObjectBuildState<BuildState> state = new();

        [MemberNotNullWhen(true, nameof(IR))]
        public bool IsBuilt => IR is not null;
    }
}
