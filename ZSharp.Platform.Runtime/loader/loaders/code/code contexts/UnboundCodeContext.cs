using CommonZ.Utils;

namespace ZSharp.Platform.Runtime.Loaders
{
    internal sealed class UnboundCodeContext(Runtime runtime, IL.Emit.ILGenerator il)
        : ICodeContext
        , IBranchingCodeContext
    {
        private readonly Mapping<IR.VM.Instruction, Emit.Label> labels = [];

        public Emit.ILGenerator IL { get; } = il;

        public CodeStack Stack { get; } = new();

        public Runtime Runtime { get; } = runtime;

        void IBranchingCodeContext.AddBranchTarget(IR.VM.Instruction target)
            => labels[target] = IL.DefineLabel();

        Emit.Label IBranchingCodeContext.GetBranchTarget(IR.VM.Instruction target)
            => labels[target];
    }
}
