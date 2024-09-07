using ZSharp.RAST;

namespace ZSharp.Compiler
{
    public sealed partial class Compiler
    {
        public ExpressionCompiler ExpressionCompiler { get; }

        public LiteralCompiler LiteralCompiler { get; }

        public StatementCompiler StatementCompiler { get; }

        public CGObject CompileNode(RDefinition definition)
        {
            foreach (var compiler in Context.CompilerStack)
                if (compiler.Compile(definition) is CGObject result)
                    return result;

            throw new NotImplementedException();
        }

        public CGObject CompileNode(RExpression expression)
        {
            return ExpressionCompiler.Compile(expression);
        }

        public CGObject CompileNode(RLiteral literal)
        {
            return LiteralCompiler.Compile(literal);
        }

        public CGObject CompileNode(RStatement statement)
        {
            foreach (var compiler in Context.CompilerStack)
            {
                if (compiler is IStatementCompiler statementCompiler &&
                    statementCompiler.Compile(statement) is CGObject result)
                    return result;
            }

            return StatementCompiler.Compile(statement);
        }

        public CG? CompileNode<CG>(RDefinition definition)
            where CG : CGObject
            => Context.GetCompilerFor<CG>().Compile(definition) as CG;

        public CG CompileNode<R, CG>(R definition)
            where R : RDefinition
            where CG : CGObject
            => Context.GetCompilerFor<R, CG>().Compile(definition);
    }
}
