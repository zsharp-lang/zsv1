using CommonZ.Utils;
using ZSharp.CGRuntime;

namespace ZSharp.Compiler
{
    public class FunctionGroup(string? name)
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
            ArgumentMatch? bestMatch = null;
            foreach (var overload in Overloads)
            {
                var match = overload.Signature.Match(compiler, args, kwArgs);

                if (match is null) continue;

                if (bestMatch is null || match > bestMatch) bestMatch = match;
            }

            throw new("No matching overload found.");
        }
    }
}
