namespace ZSharp.Compiler.EvaluatorDispatchers.IR
{
    public partial class Dispatcher(
        Compiler compiler, 
        Platform.Runtime.Runtime runtime,
        Importer.RT.Loader loader
    )
    {
        private readonly Compiler compiler = compiler;
        private readonly Platform.Runtime.Runtime runtime = runtime;
        private readonly Importer.RT.Loader loader = loader;

        private Evaluator @base;

        public void Apply()
        {
            @base = compiler.Evaluator.Clone();

            var evaluator = compiler.Evaluator;

            evaluator.Evaluate = Evaluate;
        }
    }
}
