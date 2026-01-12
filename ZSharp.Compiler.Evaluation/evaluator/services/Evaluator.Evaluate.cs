namespace ZSharp.Compiler
{
    public delegate IResult Evaluate(CompilerObject @object);

    partial struct Evaluator
    {
        public Evaluate Evaluate { get; set; } = Dispatcher.Evaluate;
    }
}
