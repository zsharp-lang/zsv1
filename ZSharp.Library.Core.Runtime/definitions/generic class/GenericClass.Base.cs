using ZSharp.Compiler.Features;

namespace Core.Runtime.Objects
{
    partial class GenericClass
        : ISingleInheritance
    {
        public CompilerObject? Base { get; set; }

        public List<CompilerObject> Interfaces { get; } = [];
    }
}
