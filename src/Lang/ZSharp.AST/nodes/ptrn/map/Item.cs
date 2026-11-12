namespace ZSharp.AST
{
    partial class MapPattern
    {
        public sealed class Item
        {
            public required Expression Value { get; set; }

            public required Pattern Pattern { get; set; }
        }
    }
}
