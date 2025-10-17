namespace ZSharp.Compiler.TSDispatchers.Static
{
    public sealed partial class Dispatcher(Compiler compiler)
    {
        private readonly Compiler compiler = compiler;
        private TS @base;

        public void Apply()
        {
            @base = compiler.TS;

            ref var ts = ref compiler.TS;

            ts.TypeOf = TypeOf;
        }
    }
}
