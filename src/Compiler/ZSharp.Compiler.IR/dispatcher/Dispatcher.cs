namespace ZSharp.Compiler
{
    internal sealed class Dispatcher
        : IIRDefinitionAsCompiler
        , IIRDefinitionInCompiler
        , IIRReferenceCompiler
    {
        internal static Dispatcher Instance { get; } = new();

        public IResult<LIR.Expression, Error> CompileExpression(CompilerObject @object, object? target)
            => Result<LIR.Expression>.Error(Unsupported($"Object [{@object}] does not support compiling to an expression."));

        public IResult<LIR.Statement, Error> CompileStatement(CompilerObject @object, object? target)
            => Result<LIR.Statement>.Error(Unsupported($"Object [{@object}] does not support compiling to a statement."));

        public IResult<LIR.Type, Error> CompileType(CompilerObject @object, object? target)
            => Result<LIR.Type>.Error(Unsupported($"Object [{@object}] does not support compiling to a type."));

        public IResult<T, Error> CompileDefinition<T>(CompilerObject @object, object? target)
            where T : LIR.Definition
            => Result<T>.Error(Unsupported($"Object [{@object}] does not define a {typeof(T).Name}."));

        public Error? CompileDefinition<Owner>(CompilerObject @object, Owner owner, object? target)
            where Owner : LIR.Definition
            => Unsupported($"Object [{@object}] can't be placed in a {typeof(Owner).Name}.");

        public IResult<T, Error> CompileReference<T>(CompilerObject @object, object? target)
            where T : class
            => Result<T>.Error(Unsupported($"Object [{@object}] does not support being referenced as {typeof(T).Name}."));

        private static UnsupportedError Unsupported(string message)
            => new(message);
    }
}
