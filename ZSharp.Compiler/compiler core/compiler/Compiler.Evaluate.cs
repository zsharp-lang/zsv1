namespace ZSharp.Compiler
{
    public sealed partial class Compiler
    {
        public List<Evaluator> Evaluators { get; } = [];

        public CompilerObject Evaluate(CompilerObject @object)
        {
            CompilerObject result = @object;

            foreach (var evaluator in Evaluators)
                if ((result = evaluator.Evaluate(result)) != @object) break;

            //while (@object == result)
            //{
            //    foreach (var evaluator in Evaluators)
            //        if ((result = evaluator.Evaluate(result)) != @object) goto loopAgain;

            //    break;

            //    loopAgain: continue;
            //}

            return result;
        }

        private void InitializeEvaluators()
        {
            Evaluators.Add(new DefaultEvaluator(this));
        }
    }
}
