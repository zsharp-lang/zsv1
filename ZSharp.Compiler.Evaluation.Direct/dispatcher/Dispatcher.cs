namespace ZSharp.Compiler.EvaluatorDispatchers.Direct
{
    public partial class Dispatcher(
        Compiler compiler
    )
    {
        private readonly Compiler compiler = compiler;

        private Evaluator @base;

        public void Apply()
        {
            @base = compiler.Evaluator;

            ref var evaluator = ref compiler.Evaluator;

            evaluator.Evaluate = Evaluate;
        }
    }
}
