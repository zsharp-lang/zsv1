namespace ZSharp.Runtime.Loaders
{
    public interface IBranchingCodeContext
        : ICodeContext
    {
        public void AddBranchTarget(IR.VM.Instruction target);

        public IL.Emit.Label GetBranchTarget(IR.VM.Instruction target);
    }
}
