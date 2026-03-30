namespace ZSharp.SourceCompiler
{
    partial class Interpreter
    {
        public required Func<AST.Statement, IResult> CompileStatement { get; init; }
    }
}
