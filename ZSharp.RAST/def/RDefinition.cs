namespace ZSharp.RAST
{
    public abstract class RDefinition : RExpression
    {
        /// <summary>
        /// The name of the definition or <see cref="null"/> if it is an anonymous definition.
        /// </summary>
        public RId Id { get; }

        public string? Name => Id.Name == string.Empty ? null : Id.Name;

        public RDefinition(string? name)
            : this(new RId(name ?? string.Empty))
        {

        }

        public RDefinition(RId? id)
        {
            Id = id;
        }
    }
}
