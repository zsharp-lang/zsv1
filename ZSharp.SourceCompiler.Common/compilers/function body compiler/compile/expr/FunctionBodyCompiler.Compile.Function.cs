namespace ZSharp.SourceCompiler
{
    partial class FunctionBodyCompiler
    {
        private IResult Compile(AST.Function function)
        {
            var compiler = new FunctionCompiler(Interpreter, function);

            var result = compiler.Declare();
            compiler.CompileSignature();
            compiler.CompileBody();

            return result;
        }
    }
}
