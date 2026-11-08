namespace Package.DotNet
{
    /// <summary>
    /// A function defined inside the code of a method, which captures the variables of
    /// the code around it by reference, as in C#. Its value exists only while that code
    /// runs, so it is a variable too.
    /// </summary>
    public sealed class LocalFunction(string name)
        : ZSharp.HIR.Definition
        , ZSharp.HIR.Instance
        , ZSharp.HIR.Variable
    {
        public string Name { get; set; } = name;

        public List<Parameter> Parameters { get; } = [];

        /// <summary>
        /// The locals of the code, each declared where it starts in the body.
        /// </summary>
        public List<ZSharp.HIR.Code.Local> Locals { get; } = [];

        /// <summary>
        /// The variables of the code around it that it uses.
        /// </summary>
        public List<ZSharp.HIR.Code.Capture> Captures { get; } = [];

        /// <summary>
        /// The returned type, or <see langword="null"/> if nothing is returned.
        /// </summary>
        public ZSharp.HIR.Type? ReturnType { get; set; }

        public ZSharp.HIR.Code.Code? Body { get; set; }
    }
}
