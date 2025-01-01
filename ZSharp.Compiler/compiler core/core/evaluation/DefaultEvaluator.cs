
namespace ZSharp.Compiler
{
    internal sealed class DefaultEvaluator(Compiler compiler) : Evaluator
    {
        public override CompilerObject Evaluate(CompilerObject @object)
            => @object is IEvaluable evaluable ? evaluable.Evaluate(compiler) : @object;
    }
}
