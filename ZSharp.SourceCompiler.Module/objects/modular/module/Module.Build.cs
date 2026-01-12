using System.Diagnostics.CodeAnalysis;

namespace ZSharp.SourceCompiler.Module.Objects
{
    partial class Module
    {
        [Flags]
        private enum BuildState
        {
            Content = 1 << 0,
            EntryPoint = 1 << 1,
            Initializer = 1 << 2,
        }

        private readonly ObjectBuildState<BuildState> state = new();

        [MemberNotNullWhen(true, nameof(IR))]
        public bool IsBuilt => IR is not null;
    }
}
