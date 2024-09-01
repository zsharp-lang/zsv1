namespace ZSharp.CGRuntime.HLVM
{
    public delegate IEnumerable<IR.VM.Instruction> Injector();

    public sealed class Inject(Injector injector)
         : Instruction
    {
        public Injector Injector { get; } = injector;

        public Inject(Func<IR.VM.Instruction> injector)
            : this(() => CreateInjector(injector))
        {

        }

        private static IEnumerable<IR.VM.Instruction> CreateInjector(Func<IR.VM.Instruction> injector)
        {
            yield return injector();
        }
    }
}
