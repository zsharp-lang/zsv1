namespace ZSharp.Compiler
{
    public delegate IResult Evaluate(CompilerObject @object);

    partial class Evaluator
    {
        public Evaluate Evaluate { get; set; } = Dispatcher.Evaluate;
    }
}
