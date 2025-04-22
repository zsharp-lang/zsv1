using CommonZ.Utils;
using ZSharp.Compiler;

namespace ZSharp.Objects
{
    public sealed class OverloadGroup(string name)
        : CompilerObject
        , ICTCallable
        , IMappable
    {
        public string Name { get; set; } = name;

        public Collection<CompilerObject> Overloads { get; init; } = [];

        public CompilerObject Call(Compiler.Compiler compiler, Argument[] arguments)
        {
            var matchingOverloads = Overloads
                .Select(overload => {
                    try { return compiler.Call(overload, arguments); }
                    catch (ArgumentMismatchException) { return null!; }
                })
                .Where(result => result is not null)
                .ToArray();

            if (matchingOverloads.Length == 0)
                throw new NoOverloadFoundException(this, arguments);

            if (matchingOverloads.Length > 1)
                throw new AmbiguousOverloadException(this, arguments, matchingOverloads);

            return matchingOverloads[0];
        }

        CompilerObject IMappable.Map(Func<CompilerObject, CompilerObject> func)
        {
            return new OverloadGroup(Name)
            {
                Overloads = [.. Overloads.Select(func).Where(item => item is not null)]
            };
        }
    }
}
