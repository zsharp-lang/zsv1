global using CommonZ;

namespace ZSharp.Compiler
{
    /// <param name="compiler">
    /// The compiler this runs in, which compiles anything inside the node. It may be a
    /// fork of the one the handler was registered on.
    /// </param>
    public delegate IResult<HIR.Code.Expression, Error> CompileASTExpression(ASTCompiler compiler, AST.Expression expression);

    /// <inheritdoc cref="CompileASTExpression"/>
    public delegate IResult<HIR.Code.Statement, Error> CompileASTStatement(ASTCompiler compiler, AST.Statement statement);

    /// <inheritdoc cref="CompileASTExpression"/>
    public delegate IResult<CompiledDefinition, Error> CompileASTDefinition(ASTCompiler compiler, AST.Definition definition);

    /// <summary>
    /// Compiles AST to HIR. Each endpoint receives the compiler it runs in, so a fork
    /// that changes an endpoint is used for everything inside the node too.
    /// </summary>
    public class ASTCompiler
    {
        public CompileASTExpression Expression { get; set; } = (_, expression)
            => Result<HIR.Code.Expression>.Error(Unsupported(expression));

        public CompileASTStatement Statement { get; set; } = (_, statement)
            => Result<HIR.Code.Statement>.Error(Unsupported(statement));

        public CompileASTDefinition Definition { get; set; } = (_, definition)
            => Result<CompiledDefinition>.Error(Unsupported(definition));

        /// <summary>
        /// The contexts of the definitions being compiled, shared by a compiler and all
        /// of its forks.
        /// </summary>
        public Contexts Contexts { get; private init; } = new();

        public IResult<HIR.Code.Expression, Error> CompileExpression(AST.Expression expression)
            => Expression(this, expression);

        public IResult<HIR.Code.Statement, Error> CompileStatement(AST.Statement statement)
            => Statement(this, statement);

        public IResult<CompiledDefinition, Error> CompileDefinition(AST.Definition definition)
            => Definition(this, definition);

        /// <summary>
        /// A new compiler with the same endpoints and the same contexts. Changing its
        /// endpoints doesn't change this one.
        /// </summary>
        public ASTCompiler Fork() => (ASTCompiler)MemberwiseClone();

        private static UnsupportedError Unsupported(AST.Node node)
            => new($"Can't compile AST node of type {node.GetType().Name}.");
    }
}
