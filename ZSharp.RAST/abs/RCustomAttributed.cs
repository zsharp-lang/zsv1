namespace ZSharp.RAST
{
    public class RCustomAttributed : RDefinition
    {
        public RCustomAttribute CustomAttribute { get; set; }

        public RDefinition Definition { get; set; }

        public RCustomAttributed(RCustomAttribute customAttribute, RDefinition definition)
            : base(definition.Id)
        {
            CustomAttribute = customAttribute;
            Definition = definition;
        }
    }
}
