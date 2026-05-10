namespace ZSharp.HIR.Code
{
    public sealed class Argument
    {
        public required Expression Value { get; set; }

        public string? Name { get; set; }
    }
}
