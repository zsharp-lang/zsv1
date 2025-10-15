namespace ZSharp.SourceCompiler
{
    partial class ExpressionCompiler
    {
        private Result<CompilerObject> Compile(AST.BinaryExpression binary)
        {
            if (
                Compile(binary.Left)
                .When(out var left)
                .Error(out var error)
            ) return Result<CompilerObject>.Error(error);

            if (binary.Operator == ".") // Special case ??? :(
            {
                if (binary.Right is AST.IdentifierExpression identifier)
                    return Interpreter.Compiler.CG.Member(left!, identifier.Name);
            }

            if (
                Compile(binary.Right)
                .When(out var right)
                .Error(out error)
            ) return Result<CompilerObject>.Error(error);

            if (!Interpreter.Operators.Op(binary.Operator, out var @operator))
                return Result<CompilerObject>.Error($"Unknown operator '{binary.Operator}'");

            return Interpreter.Compiler.CG.Call(@operator, [ new(left!), new(right!) ]);
        }
    }
}
