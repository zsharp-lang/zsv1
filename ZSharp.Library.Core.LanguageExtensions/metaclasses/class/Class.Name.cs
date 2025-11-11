using ZSharp.Compiler;

namespace Core.LanguageExtensions.Objects
{
    partial class Class
        : IHasName
    {
        public string Name { get; set; } = string.Empty;
    }
}
