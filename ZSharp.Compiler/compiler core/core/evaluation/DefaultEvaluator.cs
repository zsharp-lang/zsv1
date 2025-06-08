
namespace ZSharp.Compiler
{
    internal sealed class DefaultEvaluator(Compiler compiler) : Evaluator
    {
        public override CompilerObjectResult Evaluate(CompilerObject @object)
            => CompilerObjectResult.Ok(
                @object is IEvaluable evaluable ? evaluable.Evaluate(compiler) : @object
            );
    }
}
