namespace ZSharp.SourceCompiler.Module
{
    partial class ClassCompiler
    {
        private Result Compile(AST.Function function)
        {
            var compiler = new FunctionCompiler(Interpreter, function);

            var result = compiler.Declare();

            tasks.AddTask(() =>
            {
                compiler.CompileSignature();

                tasks.AddTask(() =>
                {
                    compiler.CompileBody();
                });
            });

            return result;
        }
    }
}
