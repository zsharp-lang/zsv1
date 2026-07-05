namespace ZSharp.SourceCompiler
{
    partial class Interpreter2
    {
        public required Func<AST.Statement, IResult> CompileStatement { get; init; }
    }
}
