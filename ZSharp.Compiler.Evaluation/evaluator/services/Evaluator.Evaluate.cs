namespace ZSharp.Compiler
{
    public delegate Result Evaluate(CompilerObject @object);

    partial struct Evaluator
    {
        public Evaluate Evaluate { get; set; } = Dispatcher.Evaluate;
    }
}
