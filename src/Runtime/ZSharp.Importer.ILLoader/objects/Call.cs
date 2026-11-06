using System.Diagnostics.CodeAnalysis;

namespace ZSharp.Importer.ILLoader.Objects
{
    internal sealed class Call(
        LIR.ICallable callable,
        CompilerObject[] arguments,
        Func<CompilerObject>? returnType
    )
        : CompilerObject
        , ICompileIRExpression
        , ITyped
    {
        CompilerObject ITyped.Type => returnType!();

        // A call that returns nothing has no type.
        bool CompilerObject.Is<T>([NotNullWhen(true)] out T? result) where T : class
            => (result = returnType is null && typeof(T) == typeof(ITyped) ? null : this as T) is not null;

        public IResult<LIR.Expression, Error> CompileIRExpression(object? target)
        {
            var values = new List<LIR.Expression>();

            foreach (var argument in arguments)
                if (
                    Core.Compiler.ModuleScope.IR
                    .CompileExpression(argument, target)
                    .When(out var value)
                    .Error(out var error)
                ) return Result<LIR.Expression>.Error(error);
                else
                    values.Add(value!);

            return Result<LIR.Expression>.Ok(new LIR.Call(callable, values));
        }
    }
}
