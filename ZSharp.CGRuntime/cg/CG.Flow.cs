namespace ZSharp.CGRuntime
{
    public static partial class CG
    {
        public static HLVM.Jump Jump(HLVM.Instruction target)
            => HLVM.Jump.Always(target);

        public static HLVM.Jump JumpIfTrue(HLVM.Instruction target)
            => HLVM.Jump.IfTrue(target);

        public static HLVM.Jump JumpIfFalse(HLVM.Instruction target)
            => HLVM.Jump.IfFalse(target);

        public static HLVM.Label Label()
            => new();

        public static HLVM.Return End()
            => new();
    }
}
