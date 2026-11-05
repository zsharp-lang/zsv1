namespace ZSharp.AST
{
    partial class ObjectPattern
    {
        public sealed class TypedItem : Item
        {
            public required string Name { get; set; }

            public required Expression Type { get; set; }
        }
    }
}
