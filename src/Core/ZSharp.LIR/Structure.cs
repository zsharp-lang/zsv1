namespace ZSharp.LIR
{
    public sealed class Structure
        : ModuleMember
        , Type
    {
        /// <summary>
        /// The fields in order. Layout is inferred from this order.
        /// </summary>
        public List<Field> Fields { get; } = [];
    }
}
