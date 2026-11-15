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

            return result;
        }

        /// <summary>
        /// The code that fills a definition is where the definition was written, but for what
        /// says where it was written itself, as a member compiled inside it does.
        /// </summary>
        private IResult<CompiledDefinition, Error> CompileDefinition(ASTCompiler current, AST.Definition definition)
        {
            var result = @base.Definition(current, definition);

            if (result.IsUnsupported())
                result = Definitions.Compile(current, definition);

            if (!result.Ok(out var compiled) || Map(definition) is not { } map)
                return result;

            var fill = compiled.Fill;

            return Result<CompiledDefinition>.Ok(compiled with
            {
                Fill = () =>
                {
                    var filled = fill();

                    if (filled.Ok(out var block))
                    {
                        Mark(current, block, map);

                        foreach (var statement in block.Statements)
                            Mark(current, statement, map);
                    }

                    return filled;
                }
            });
        }

        private IResult<HIR.Code.Statement, Error> CompileStatement(ASTCompiler current, AST.Statement statement)
        {
            var result = @base.Statement(current, statement);

            if (result.IsUnsupported())
                result = Statements.Compile(current, statement);

            if (result.Ok(out var compiled) && Map(statement) is { } map)
                Mark(current, compiled, map);

            return result;
        }

        private static void Mark(ASTCompiler current, HIR.Code.Statement statement, SourceMap map)
            => current.Maps[statement] ??= map;

        /// <summary>
        /// Where a node was written, as what is compiled from it carries it, in the maps of
        /// the compiler it is compiled in. What the innermost compile gives keeps its own:
        /// a statement one compile gives back for another is where it was first compiled
        /// from.
        /// </summary>
        private static SourceMap? Map(AST.Node node)
            => node.Location is { Path: { } path, Span: var span }
                ? new(path, span.Start.Line, span.Start.Column, span.End.Line, span.End.Column)
                : null;
    }
}
