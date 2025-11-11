namespace Core.LanguageExtensions.Objects
{
    partial class Class
    {
        public CompilerObject? Base { get; set; }

        public List<CompilerObject> Interfaces { get; } = [];
    }
}
