namespace ZSharp.CGObjects
{
    public sealed class Parameter(string name) : CGObject
    {
        public IR.Parameter? IR { get; set; }

        public string Name { get; } = name;

        public CGCode? Type { get; init; }

        public CGCode? Initializer { get; init;}
    }
}
