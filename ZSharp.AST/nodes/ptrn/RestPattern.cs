namespace ZSharp.AST
{
    public sealed class RestPattern : Pattern
    {
        private static readonly RestPattern discard = new RestPattern
        {
            Name = string.Empty
        };

        public required string Name { get; set; }

        public bool IsDiscard => string.IsNullOrEmpty(Name);

        public static RestPattern Discard => discard;
    }
}
