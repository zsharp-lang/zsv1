namespace ZSharp.RAST
{
    public class ROOPDefinition : RDefinition
    {
        public string Type { get; set; }

        public RExpression? MetaType { get; set; }

        public List<RGenericParameter>? GenericParameters { get; set; }

        public List<RParameter>? Parameters { get; set; }

        public List<RExpression>? Bases { get; set; }

        public RBlock? Content { get; set; }

        public ROOPDefinition(string type, string? name)
            : this(type, name is null ? null : new RId(name))
        {
            
        }

        public ROOPDefinition(string type, RId? id)
            : base(id)
        {
            Type = type;
        }
    }
}
