namespace Package.DotNet
{
    public sealed class Method(string name)
        : ZSharp.HIR.Definition
        , ZSharp.HIR.Instance
    {
        public string Name { get; set; } = name;

        public bool IsStatic { get; set; }

        public List<Parameter> Parameters { get; } = [];

        /// <summary>
        /// The returned type, or <see langword="null"/> if nothing is returned.
        /// </summary>
        public ZSharp.HIR.Type? ReturnType { get; set; }

        /// <summary>
        /// The code of a method being defined. A method that already exists, or
        /// that is abstract, has none.
        /// </summary>
        public ZSharp.HIR.Code.Code? Body { get; set; }
    }
}
