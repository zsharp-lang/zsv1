namespace ZSharp.Runtime.NET.IR2IL.Code
{
    public interface IBranchingCodeContext
        : ICodeContext
    {
        public void AddBranchTarget(IR.VM.Instruction target);

        public IL.Emit.Label GetBranchTarget(IR.VM.Instruction target);
    }
}
