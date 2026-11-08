namespace Package.DotNet
{
    public sealed class Method(string name)
        : ZSharp.HIR.Definition
        , ZSharp.HIR.Instance
    {
        public string Name { get; set; } = name;

        public bool IsStatic { get; set; }

        /// <summary>
        /// Whether a derived class can override it.
        /// </summary>
        public bool IsVirtual { get; set; }

        /// <summary>
        /// Whether it has no code of its own, so a derived class must override it.
        /// </summary>
        public bool IsAbstract { get; set; }

        /// <summary>
        /// Whether it overrides a virtual method of a base class.
        /// </summary>
        public bool IsOverride { get; set; }

        /// <summary>
        /// The object an instance method runs on.
        /// </summary>
        public This This { get; } = new();

        public List<CustomMetadata> CustomMetadata { get; } = [];

        public List<ZSharp.HIR.Code.Parameter> Parameters { get; } = [];

        /// <summary>
        /// The locals of the code, each declared where it starts in the body.
        /// </summary>
        public List<ZSharp.HIR.Code.Local> Locals { get; } = [];

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
