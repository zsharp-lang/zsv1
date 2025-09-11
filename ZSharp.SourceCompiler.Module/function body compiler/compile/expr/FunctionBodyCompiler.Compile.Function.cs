namespace ZSharp.SourceCompiler.Module
{
    partial class FunctionBodyCompiler
    {
        private Result Compile(AST.Function function)
        {
            var compiler = new FunctionCompiler(Interpreter, function);

            var result = compiler.Declare();
            compiler.CompileSignature();
            compiler.CompileBody();

            return result;
        }
    }
}
