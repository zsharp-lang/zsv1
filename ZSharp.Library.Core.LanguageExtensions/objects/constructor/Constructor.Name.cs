using ZSharp.Compiler;

namespace Core.LanguageExtensions.Objects
{
    partial class Constructor
        : IHasName
    {
        public string Name { get; set; } = string.Empty;
    }
}
