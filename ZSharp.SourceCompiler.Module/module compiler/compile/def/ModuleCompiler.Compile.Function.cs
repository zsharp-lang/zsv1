namespace ZSharp.SourceCompiler.Module
{
    partial class ModuleCompiler
    {
        private IResult Compile(AST.Function function)
        {
            var compiler = new FunctionCompiler(Interpreter, function);

            var result = compiler.Declare();

            if (result.Ok(out var definition))
                Object.Content.Add(definition);
            if (
                function.Name != string.Empty
                && (result = Object.AddMember(function.Name, definition!))
                .IsError
            )
                return result;

            tasks.AddTask(compiler.CompileSignature);
            tasks.AddTask(compiler.CompileBody);

            return result;
        }
    }
}
