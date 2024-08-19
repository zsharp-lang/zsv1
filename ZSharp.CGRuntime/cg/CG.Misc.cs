namespace ZSharp.CGRuntime
{
    public static partial class CG
    {
        public static HLVM.Inject Inject(HLVM.Injector injector)
            => new(injector);

        public static HLVM.Inject Inject(Func<IR.VM.Instruction> injector)
            => new(injector);

        public static HLVM.Definition Definition()
            => new();

        public static HLVM.Evaluate Evaluate()
            => new();
    }
}
