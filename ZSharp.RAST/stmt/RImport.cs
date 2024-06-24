namespace ZSharp.RAST
{
    public class RImport : RStatement
    {
        public RId? As { get; set; }

        public List<RArgument>? Arguments { get; set; }

        public RPattern? Target { get; set; }

        public RExpression Source { get; set; }

        public RImport(RExpression source)
        {
            Source = source;
        }
    }
}
