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
        /// Takes the positional arguments left, in order, as a positional rest parameter
        /// does.
        /// </summary>
        public CompilerObject[] RestPositional()
        {
            CompilerObject[] rest = [.. positional];
            positional.Clear();
            return rest;
        }

        /// <summary>
        /// Takes the named arguments left, by name, as a named rest parameter does.
        /// </summary>
        public IReadOnlyDictionary<string, CompilerObject> RestNamed()
        {
            var rest = new Dictionary<string, CompilerObject>(named);
            named.Clear();
            return rest;
        }
    }
}
