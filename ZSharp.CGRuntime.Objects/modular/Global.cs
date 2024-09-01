namespace ZSharp.CGObjects
{
    public sealed class Global(string name)
        : CGObject
    {
        public IR.Global? IR { get; set; }

        public string Name { get; } = name;

        public CGCode? Initializer { get; set; }

        public CGCode? Type { get; set; }
    }
}
