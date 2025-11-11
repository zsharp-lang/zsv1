namespace ZSharp.SourceCompiler.Module
{
    partial class ModuleCompiler
    {
        private IResult Compile(AST.OOPDefinition oop)
        {
            if (oop.Type != "class")
                return Result.Error("Only class definitions are currently supported in this context.");

            var compiler = new Class.ClassCompiler(Interpreter, oop);

            var result = compiler.Declare();

            tasks.AddTask(compiler.Compile);

            if (result.Ok(out var definition))
                Object.Content.Add(definition);
            if (
                oop.Name != string.Empty
                && (result = Object.AddMember(oop.Name, definition!))
                .IsError
            )
                return result;

            return result;
        }
    }
}
