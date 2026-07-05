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
            @base = compiler.Evaluator.Clone();

            var evaluator = compiler.Evaluator;

            evaluator.Evaluate = Evaluate;
        }
    }
}
