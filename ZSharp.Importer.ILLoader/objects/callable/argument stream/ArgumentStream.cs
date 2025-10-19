using ZSharp.Compiler;
using ZSharp.Compiler.Features.Callable;

namespace ZSharp.Importer.ILLoader.Objects
{
    internal sealed class ArgumentStream
        : IArgumentStream
    {
        internal readonly List<CompilerObject> args = [];
        private readonly Dictionary<string, CompilerObject> kwArgs = [];

        public ArgumentStream(Argument[] arguments)
        {
            foreach (var argument in arguments)
                if (argument.Name is null) args.Add(argument.Object);
                else kwArgs[argument.Name] = argument.Object;
        }

        bool IArgumentStream.HasArguments => args.Count > 0 || kwArgs.Count > 0;

        CompilerObject? IArgumentStream.PopArgument()
        {
            if (args.Count == 0) return null;
            var arg = args[0];
            args.RemoveAt(0);
            return arg;
        }

        CompilerObject? IArgumentStream.PopArgument(string name)
        {
            if (!kwArgs.Remove(name, out var arg)) return null;
            return arg;
        }

        public bool HasArgument(string name)
            => kwArgs.ContainsKey(name);
    }
}
