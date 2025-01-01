namespace ZSharp.Compiler
{
    public interface IEvaluable
    {
        public CompilerObject Evaluate(Compiler compiler);
    }
}
