namespace ZSharp.RAST
{
    public sealed class ROverloadGroup : RDefinition
    {
        public List<RDefinition> Overloads { get; } = new();

        public ROverloadGroup(string name)
            : this(new RId(name))
        {

        }

        public ROverloadGroup(RId id)
            : base(id)
        {

        }
    }
}
