using CommonZ.Utils;

namespace ZSharp.Runtime.NET.IR2IL.Code
{
    internal sealed class UnboundCodeContext(IRLoader loader, IL.Emit.ILGenerator il)
        : ICodeContext
        , IBranchingCodeContext
    {
        private readonly Mapping<IR.VM.Instruction, IL.Emit.Label> labels = [];

        public IL.Emit.ILGenerator IL { get; } = il;

        public CodeStack Stack { get; } = new();

        public IRLoader Loader { get; } = loader;

        void IBranchingCodeContext.AddBranchTarget(IR.VM.Instruction target)
            => labels[target] = IL.DefineLabel();

        IL.Emit.Label IBranchingCodeContext.GetBranchTarget(IR.VM.Instruction target)
            => labels[target];
    }
}
