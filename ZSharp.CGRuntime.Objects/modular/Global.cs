namespace ZSharp.Compiler
{
    public sealed class Global(string name)
        : CGObject
    {
        public string Name { get; } = name;

        public CGCode? Initializer { get; set; }

        public CGCode? Type { get; set; }
    }
}
