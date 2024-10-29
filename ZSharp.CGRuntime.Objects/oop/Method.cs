using ZSharp.Compiler;

namespace ZSharp.CGObjects
{
    public sealed class Method(string? name)
        : CGObject
    {
        public IR.Method? IR { get; set; }

        public string? Name { get; set; } = name;
    }
}
