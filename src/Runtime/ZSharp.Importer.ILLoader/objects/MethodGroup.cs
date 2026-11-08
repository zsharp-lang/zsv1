using System.Reflection;

namespace ZSharp.Importer.ILLoader.Objects
{
    /// <summary>
    /// The candidates a name or a constructor call stands for, optionally bound to
    /// the object they are called on.
    /// </summary>
    internal sealed class MethodGroup(ILLoader loader, MethodBase[] candidates, CompilerObject? receiver)
        : CompilerObject
        , ICTCallable
        , ICompileIRReference<LIR.ICallable>
    {
        public IResult Call(Argument[] arguments)
        {
            var matching = candidates
                .Select(candidate => (Method: candidate, Values: Convert(candidate, arguments)))
                .Where(candidate => candidate.Values is not null)
                .ToArray();

            if (matching.Length > 1)
                matching = [.. matching.Where(candidate => Exact(candidate.Method, arguments))];

            if (matching is not [var (method, values)])
                return Result.Error(
                    $"{candidates[0].Name} has {matching.Length} matching overloads for {arguments.Length} arguments."
                );

            return Result.Ok(new Call(
                loader.Handle(method),
                [.. Receiver(), .. values!],
                ILLoader.ReturnType(method) is { } type ? () => loader.Class(type) : null
            ));
        }

        public IResult<LIR.ICallable, Error> CompileIRReference(object? target)
            => candidates is [var method]
                ? Result<LIR.ICallable>.Ok(loader.Handle(method))
                : Result<LIR.ICallable>.Error($"{candidates[0].Name} is overloaded.");

        private IEnumerable<CompilerObject> Receiver()
            => receiver is null ? [] : [receiver];

        private CompilerObject[]? Convert(MethodBase candidate, Argument[] arguments)
        {
            var parameters = candidate.GetParameters();

            return parameters.Length == arguments.Length
                ? Arguments.Convert(parameters.Select(parameter => loader.Class(parameter.ParameterType)), arguments)
                : null;
        }

        private bool Exact(MethodBase candidate, Argument[] arguments)
            => candidate.GetParameters().Zip(arguments).All(pair =>
                Core.Compiler.ModuleScope.TS.TypeOf(pair.Second.Object).Ok(out var type)
                && Same(loader.Class(pair.First.ParameterType), type));

        private static bool Same(CompilerObject a, CompilerObject b)
            => Core.Compiler.ModuleScope.TS.IsAssignableTo(a, b) && Core.Compiler.ModuleScope.TS.IsAssignableTo(b, a);
    }
}
