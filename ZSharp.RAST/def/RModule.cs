namespace ZSharp.RAST
{
    public class RModule : RDefinition
    {
        public RBlock? Content { get; set; }

        public RModule(string name)
            : this(new RId(name))
        {

        }

        public RModule(RId id)
            : base(id)
        {

        }
    }
}
