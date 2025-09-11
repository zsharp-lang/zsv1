using System.Diagnostics.CodeAnalysis;

namespace ZSharp.SourceCompiler.Module.Objects
{
    partial class Module
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
