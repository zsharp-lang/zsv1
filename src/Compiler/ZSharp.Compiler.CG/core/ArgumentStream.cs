namespace ZSharp.Compiler
{
    /// <summary>
    /// The arguments of a call as its parameters take them: positional ones in order, and
    /// named ones by name. Each parameter takes what it needs, and whatever is left when
    /// they are done is too many.
    /// </summary>
    public sealed class ArgumentStream
    {
        private readonly List<CompilerObject> positional = [];
        private readonly Dictionary<string, CompilerObject> named = [];

        public ArgumentStream(IEnumerable<Argument> arguments)
        {
            foreach (var argument in arguments)
                if (argument.Name is null)
                    positional.Add(argument.Object);
                else
                    named[argument.Name] = argument.Object;
        }

        public bool HasArguments => positional.Count > 0 || named.Count > 0;

        /// <summary>
        /// Takes the next positional argument, if there is one.
        /// </summary>
        public CompilerObject? Pop()
        {
            if (positional.Count == 0)
                return null;

            var argument = positional[0];
            positional.RemoveAt(0);
            return argument;
        }

        /// <summary>
        /// Takes the argument with a name, if there is one.
        /// </summary>
        public CompilerObject? Pop(string name)
            => named.Remove(name, out var argument) ? argument : null;

        /// <summary>
        /// Takes every argument left, the positional ones in order and then the named ones,
        /// as a parameter that takes the rest does.
        /// </summary>
        public Argument[] Rest()
        {
            Argument[] rest = [
                .. positional.Select(argument => new Argument(argument)),
                .. named.Select(pair => new Argument(pair.Key, pair.Value)),
            ];

            positional.Clear();
            named.Clear();

            return rest;
        }
    }
}
