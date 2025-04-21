namespace ZSharp.Runtime.NET.IR2IL
{
    internal sealed class Parameter
    {
        public required string Name { get; set; }

        public required Type Type { get; set; }

        public int Position { get; set; }
    }
}
