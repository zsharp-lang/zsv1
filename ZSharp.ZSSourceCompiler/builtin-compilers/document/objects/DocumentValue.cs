namespace ZSharp.ZSSourceCompiler
{
    public sealed class DocumentValue(string name, CompilerObject value)
        : CompilerObject
        , Compiler.ICTAssignable
        , Compiler.IEvaluable
    {
        public string Name { get; } = name;

        public CompilerObject Value { get; set; } = value;

        public bool IsReadOnly { get; set; }

        CompilerObject Compiler.ICTAssignable.Assign(Compiler.Compiler compiler, CompilerObject value)
            => IsReadOnly ? throw new InvalidOperationException() : Value = compiler.Evaluate(value);

        public CompilerObject Evaluate(Compiler.Compiler compiler)
            => Value;

    }
}
