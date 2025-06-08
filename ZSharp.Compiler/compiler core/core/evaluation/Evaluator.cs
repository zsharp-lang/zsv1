namespace ZSharp.Compiler
{
    public abstract class Evaluator
    {
        public abstract CompilerObjectResult Evaluate(CompilerObject @object);
    }
}
