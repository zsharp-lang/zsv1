namespace ZSharp.CGRuntime.HLVM
{
    public sealed class Jump(Instruction target, JumpCondition condition)
        : Instruction
    {
        public Instruction Target { get; } = target;

        public JumpCondition Condition { get; } = condition;

        public static Jump Always(Instruction target)
            => new(target, JumpCondition.Always);

        public static Jump IfTrue(Instruction target)
            => new(target, JumpCondition.IfTrue);

        public static Jump IfFalse(Instruction target)
            => new(target, JumpCondition.IfFalse);
    }
}
