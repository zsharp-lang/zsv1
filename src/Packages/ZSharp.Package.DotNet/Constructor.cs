namespace Package.DotNet
{
    /// <summary>
    /// Initializes a new instance of its type.
    /// </summary>
    public sealed class Constructor
        : ZSharp.HIR.Definition
        , ZSharp.HIR.Instance
        , Accessible
        , ZSharp.HIR.Annotated
    {
        public Accessibility Accessibility { get; set; } = Accessibility.Public;

        public List<object> Annotations { get; } = [];

        public This This { get; } = new();

        public List<CustomMetadata> CustomMetadata { get; } = [];

        public List<ZSharp.HIR.Code.Parameter> Parameters { get; } = [];

        /// <summary>
        /// The locals of the code, each declared where it starts in the body.
        /// </summary>
        public List<ZSharp.HIR.Code.Local> Locals { get; } = [];

        /// <summary>
        /// The code of a constructor being defined. One that already exists has none.
        /// </summary>
        public ZSharp.HIR.Code.Code? Body { get; set; }
    }
}
