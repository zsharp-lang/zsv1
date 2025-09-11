using System.Diagnostics.CodeAnalysis;

namespace ZSharp.SourceCompiler.Module.Objects
{
    partial class Function
    {
        [Flags]
        private enum BuildState
        {

        }

        private readonly ObjectBuildState<BuildState> state = new();

        [MemberNotNullWhen(true, nameof(IR))]
        public bool IsBuilt => IR is not null;
    }
}
