namespace Package.DotNet
{
    /// <summary>
    /// Initializes a new instance of its type.
    /// </summary>
    public sealed class Constructor
        : ZSharp.HIR.Definition
        , ZSharp.HIR.Instance
    {
        public This This { get; } = new();

        public List<CustomMetadata> CustomMetadata { get; } = [];

        public List<Parameter> Parameters { get; } = [];

        /// <summary>
        /// The code of a constructor being defined. One that already exists has none.
        /// </summary>
        public ZSharp.HIR.Code.Code? Body { get; set; }
    }
}
