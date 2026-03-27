namespace ZSharp.SourceCompiler.Class
{
    partial class ClassCompiler
    {
        private IResult Compile(AST.Constructor constructor)
        {
            //return Result.Error("Function definitions are not yet supported in class definitions.");
            var compiler = new ConstructorCompiler(Interpreter, constructor);

            var result = compiler.Declare();

            if (result.Ok(out var definition))
                Spec.Content.Add(definition);

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
