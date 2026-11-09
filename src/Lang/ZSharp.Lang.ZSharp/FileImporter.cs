using ZSharp.Importer.String;

namespace ZSharp.Lang
{
    /// <summary>
    /// Imports a file by its path, which is relative to the document importing it. What a
    /// file means is up to what is registered for its extension: a <c>.zs</c> file is a
    /// document, and anything can register another kind, as Z# does for a <c>.dll</c>.
    /// </summary>
    public sealed class FileImporter(ASTCompiler compiler)
        : IStringImporter
    {
        private readonly Dictionary<string, Func<string, IResult<HIR.Instance, Error>>> kinds
            = new(StringComparer.OrdinalIgnoreCase);

        /// <summary>
        /// Makes files with an extension, as <c>.dll</c>, imported by a function, which is
        /// given the file's full path.
        /// </summary>
        public void Register(string extension, Func<string, IResult<HIR.Instance, Error>> import)
            => kinds[extension] = import;

        public IResult<HIR.Instance, Error> Import(string source)
        {
            var path = Path.GetFullPath(Resolve(source));

            return kinds.TryGetValue(Path.GetExtension(path), out var import)
                ? import(path)
                : Result<HIR.Instance>.Error($"Nothing imports a file like {path}, as nothing is registered for its extension.");
        }

        private string Resolve(string source)
            => Path.IsPathRooted(source)
                ? source
                : Path.Combine(
                    compiler.Contexts.Has<IDocument>(out var importing) && Path.GetDirectoryName(importing.Document.Path) is { Length: > 0 } directory
                        ? directory
                        : Environment.CurrentDirectory,
                    source
                );
    }
}
