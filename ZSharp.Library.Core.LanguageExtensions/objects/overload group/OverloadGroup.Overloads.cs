namespace Core.LanguageExtensions.Objects
{
    partial class OverloadGroup
    {
        private readonly List<CompilerObject> overloads = [];

        public void AddOverload(CompilerObject @object)
        {
            overloads.Add(@object);
        }
    }
}
