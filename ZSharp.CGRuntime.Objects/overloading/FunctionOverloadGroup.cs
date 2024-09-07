using CommonZ.Utils;
using ZSharp.Compiler;

namespace ZSharp.CGObjects
{
    public class FunctionOverloadGroup(string? name)
        : CGObject
        //, ICTCallable
    {
        public string? Name { get; set; } = name;

        public Collection<Function> Overloads { get; } = [];

        public CGObject Call(ICompiler compiler, Argument[] arguments)
        {
            var (args, kwargs) = Utils.SplitArguments(arguments);
            return Call(compiler, args, kwargs);
        }

        public CGObject Call(ICompiler compiler, Args args, KwArgs kwArgs)
        {
            if (Overloads.Count == 0)
                throw new("Empty overload group.");

            var bestMatch =
                    Overloads
                    .Select(overload => overload.Match(compiler, args, kwArgs))
                    .Where(match => match is not null)
                    .Max();

            throw new("No matching overload found.");
        }
    }
}
