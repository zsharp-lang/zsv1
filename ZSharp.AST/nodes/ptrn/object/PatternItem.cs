namespace ZSharp.AST
{
    partial class ObjectPattern
    {
        public sealed class PatternItem : Item
        {
            public required string Name { get; set; }

            public Pattern? Value { get; set; }
        }
    }
}
