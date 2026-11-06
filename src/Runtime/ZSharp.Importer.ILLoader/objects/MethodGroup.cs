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
                .Where(candidate => candidate.GetParameters().Length == arguments.Length)
                .ToArray();

            if (matching.Length > 1)
                matching = [.. matching.Where(candidate => Accepts(candidate, arguments))];

            if (matching.Length > 1)
                matching = [.. matching.Where(candidate => Exact(candidate, arguments))];

            if (matching is not [var method])
                return Result.Error(
                    $"{candidates[0].Name} has {matching.Length} matching overloads for {arguments.Length} arguments."
                );

            return Result.Ok(new Call(
                loader.Handle(method),
                [.. Receiver(), .. arguments.Select(argument => argument.Object)],
                ILLoader.ReturnType(method) is { } type ? () => loader.Class(type) : null
            ));
        }

        public IResult<LIR.ICallable, Error> CompileIRReference(object? target)
            => candidates is [var method]
                ? Result<LIR.ICallable>.Ok(loader.Handle(method))
                : Result<LIR.ICallable>.Error($"{candidates[0].Name} is overloaded.");

        private IEnumerable<CompilerObject> Receiver()
            => receiver is null ? [] : [receiver];

        private static bool Accepts(MethodBase candidate, Argument[] arguments)
            => candidate.GetParameters().Zip(arguments).All(pair =>
                TypeOf(pair.Second) is not { } type || pair.First.ParameterType.IsAssignableFrom(type));

        private static bool Exact(MethodBase candidate, Argument[] arguments)
            => candidate.GetParameters().Zip(arguments).All(pair =>
                TypeOf(pair.Second) == pair.First.ParameterType);

        private static Type? TypeOf(Argument argument)
            => argument.Object.Is<ITyped>(out var typed) && typed.Type.Is<Class>(out var type)
                ? type.IL
                : null;
    }
}
