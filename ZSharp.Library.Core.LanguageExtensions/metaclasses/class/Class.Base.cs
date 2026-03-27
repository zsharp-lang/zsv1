using ZSharp.Compiler.Features;

namespace Core.LanguageExtensions.Objects
{
    partial class Class 
        : ISingleInheritance
    {
        public CompilerObject? Base { get; set; }

        public List<CompilerObject> Interfaces { get; } = [];
    }
}
