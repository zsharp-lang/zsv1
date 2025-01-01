namespace ZSharp.Compiler
{
    public abstract class Evaluator
    {
        public abstract CompilerObject Evaluate(CompilerObject @object);
    }
}
