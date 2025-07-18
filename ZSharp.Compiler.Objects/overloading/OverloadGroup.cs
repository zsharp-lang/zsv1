using CommonZ.Utils;
using ZSharp.Compiler;

namespace ZSharp.Objects
{
    public abstract class OverloadGroup<T>(string name)
        : CompilerObject
        , ICTCallable_Old
        , IMappable

        where T : CompilerObject
    {
        public string Name { get; set; } = name;

        public Collection<T> Overloads { get; init; } = [];

        public virtual CompilerObject Call(Compiler.Compiler compiler, Argument[] arguments)
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
                Overloads = [.. Overloads.Select(item => func(item)).Where(item => item is not null)]
            };
        }
    }

    public sealed class OverloadGroup(string name)
        : OverloadGroup<CompilerObject>(name)
        , CompilerObject
        , ICTCallable_Old
        , IMappable
    {


    }
}
