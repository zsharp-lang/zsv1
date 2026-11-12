namespace ZSharp.Compiler
{
    /// <summary>
    /// Gives the value of a parameter for a call, taking what it needs from the call's
    /// arguments. How a parameter gets its value is up to it: from an argument, a default,
    /// or somewhere else entirely, as an injected parameter does.
    /// </summary>
    public delegate IResult ArgumentMatch(CompilerObject parameter, ArgumentStream arguments);

    partial class Matching
    {
        public ArgumentMatch Parameter { get; set; } = Dispatcher.Parameter;

        /// <summary>
        /// The value of a parameter for a call, from its arguments.
        /// </summary>
        public IResult Match(CompilerObject parameter, ArgumentStream arguments)
            => Parameter(parameter, arguments);
    }
}
