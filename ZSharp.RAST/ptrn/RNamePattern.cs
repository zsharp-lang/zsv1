namespace ZSharp.RAST
{
    public sealed class RNamePattern : RPattern
    {
        public string Name
        {
            get => Id.Name;
            set => Id.Name = value;
        }

        public RId Id { get; set; }

        public RExpression? Type { get; set; }

        public RNamePattern(RId id)
        {
            Id = id;
        }
    }
}
