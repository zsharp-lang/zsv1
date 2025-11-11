using ZSharp.Compiler;

namespace Core.LanguageExtensions.Objects
{
    partial class Method
        : IHasName
    {
        public string Name { get; set; } = string.Empty;
    }
}
