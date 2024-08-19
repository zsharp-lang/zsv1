namespace ZSharp.RAST
{
    public class RImportTarget(string name, RId? alias = null) : RDefinition(name)
    {
        public RId? Alias { get; set; } = alias;
    }

    public class RImport : RStatement
    {
        public RId? As { get; set; }

        public List<RArgument>? Arguments { get; set; }

        public List<RImportTarget>? Targets { get; set; }

        public RExpression Source { get; set; }

        public RImport(RExpression source)
        {
            Source = source;
        }
    }
}
