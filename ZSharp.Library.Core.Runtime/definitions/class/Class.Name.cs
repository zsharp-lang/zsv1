using ZSharp.Compiler;

namespace Core.Runtime.Objects
{
    partial class Class
        : IHasName
    {
        public string Name { get; set; } = string.Empty;
    }
}
