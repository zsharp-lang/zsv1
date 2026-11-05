namespace ZSharp.Importer.ILLoader.Objects
{
    internal sealed class Call(LIR.ICallable callable, Argument[] arguments)
        : CompilerObject
        , ICompileIRExpression
    {
        public IResult<LIR.Expression, Error> CompileIRExpression(object? target)
        {
            var values = new List<LIR.Expression>();

            foreach (var argument in arguments)
                if (
                    Core.Compiler.ModuleScope.IR
                    .CompileExpression(argument.Object, target)
                    .When(out var value)
                    .Error(out var error)
                ) return Result<LIR.Expression>.Error(error);
                else
                    values.Add(value!);

            return Result<LIR.Expression>.Ok(new LIR.Call(callable, values));
        }
    }
}
