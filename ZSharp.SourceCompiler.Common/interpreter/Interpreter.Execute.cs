namespace ZSharp.SourceCompiler
{
    partial class Interpreter2
    {
        public Error? Execute(AST.Statement statement)
        {
            if (
                CompileStatement(statement)
                .When(out var co)
                .Error(out var error)
                ||
                RootInterpreter.Evaluate(co!)
                .Error(out error)
            ) return error;

            return null;
        }
    }
}
