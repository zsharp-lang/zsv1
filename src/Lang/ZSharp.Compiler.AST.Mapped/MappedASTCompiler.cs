using ZSharp.SourceMapping;

namespace ZSharp.Compiler
{
    /// <summary>
    /// Implements <see cref="ASTCompiler"/> through registered compilers.
    /// </summary>
    public sealed class MappedASTCompiler(ASTCompiler compiler)
    {
        private readonly ASTCompiler compiler = compiler;

        private ASTCompiler @base = null!;

        public ASTMap<AST.Expression, HIR.Code.Expression> Expressions { get; } = new();

        public ASTMap<AST.Statement, HIR.Code.Statement> Statements { get; } = new();

        public ASTMap<AST.Definition, CompiledDefinition> Definitions { get; } = new();

        public void Apply()
        {
            @base = compiler.Fork();

            compiler.Expression = CompileExpression;
            compiler.Statement = CompileStatement;
            compiler.Definition = CompileDefinition;
        }

        private IResult<HIR.Code.Expression, Error> CompileExpression(ASTCompiler current, AST.Expression expression)
        {
            var result = @base.Expression(current, expression);

            if (result.IsUnsupported())
                result = Expressions.Compile(current, expression);

            if (result.Ok(out var compiled) && Map(expression) is { } map && SourceMaps.Of(compiled) is null)
                SourceMaps.Set(compiled, map);

            return result;
        }

        private IResult<CompiledDefinition, Error> CompileDefinition(ASTCompiler current, AST.Definition definition)
        {
            var result = @base.Definition(current, definition);

            if (result.IsUnsupported())
                result = Definitions.Compile(current, definition);

            return result;
        }

        private IResult<HIR.Code.Statement, Error> CompileStatement(ASTCompiler current, AST.Statement statement)
        {
            var result = @base.Statement(current, statement);

            if (result.IsUnsupported())
                result = Statements.Compile(current, statement);

            if (result.Ok(out var compiled) && Map(statement) is { } map && SourceMaps.Of(compiled) is null)
                SourceMaps.Set(compiled, map);

            return result;
        }

        /// <summary>
        /// Where a node was written, as what is compiled from it carries it. What the
        /// innermost compile gives keeps its own: a node one compile gives back for
        /// another, as a name gives the same expression wherever it is used, is where it
        /// was first compiled from.
        /// </summary>
        private static SourceMap? Map(AST.Node node)
            => node.Location is { Path: { } path, Span: var span }
                ? new(path, span.Start.Line, span.Start.Column, span.End.Line, span.End.Column)
                : null;
    }
}
