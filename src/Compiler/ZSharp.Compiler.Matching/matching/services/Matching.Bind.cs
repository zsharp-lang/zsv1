namespace ZSharp.Compiler
{
    /// <summary>
    /// Binds a parameter for a call: gives the value it takes from the call's arguments. How
    /// a parameter gets its value is up to it: from an argument, a default, or somewhere
    /// else entirely, as an injected parameter does.
    /// </summary>
    public delegate IResult BindParameter(CompilerObject parameter, ArgumentStream arguments);

    partial class Matching
    {
        public BindParameter BindParameter { get; set; } = Dispatcher.Bind;

        /// <summary>
        /// The value a parameter takes from a call's arguments.
        /// </summary>
        public IResult Bind(CompilerObject parameter, ArgumentStream arguments)
            => BindParameter(parameter, arguments);
    }
}
