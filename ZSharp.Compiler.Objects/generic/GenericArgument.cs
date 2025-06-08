using ZSharp.Compiler;

namespace ZSharp.Objects
{
    public sealed class GenericArgument(IType type)
    {
        public IType Type { get; set; } = type;

        public string? Name { get; set; }

        public GenericArgument(string name, IType type)
            : this(type)
        {
            Name = name;
        }
    }
}
