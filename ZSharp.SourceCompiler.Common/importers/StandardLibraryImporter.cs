namespace ZSharp.SourceCompiler
{
    public sealed class StandardLibraryImporter
        : IStringImporter
    {
        private readonly Dictionary<string, CompilerObject> libraries = [];

        public void Add(string name, CompilerObject obj)
            => libraries.Add(name, obj);

        Result<CompilerObject> IStringImporter.Import(string source)
        {
            if (libraries.TryGetValue(source, out var result))
                return Result<CompilerObject>.Ok(result);

            return Result<CompilerObject>.Error($"Could not find standard library '{source}'");
        }
    }
}
