using ZSharp.Importer.String;
using ZSharp.Parser;
using ZSharp.Tokenizer;

namespace ZSharp.Lang
{
    /// <summary>
    /// Imports a Z# document by its path, which is relative to the document importing it.
    /// Each document runs once, and what it exports is what importing it gives.
    /// </summary>
    public sealed class DocumentImporter(Language language, ASTCompiler compiler, IDocumentHost host)
        : IStringImporter
    {
        private readonly Dictionary<string, Package.ZSharp.Document> documents = [];
        private readonly HashSet<string> running = [];

        public IResult<HIR.Instance, Error> Import(string source)
            => Run(Resolve(source)).When(out var document).Error(out var error)
                ? Result<HIR.Instance>.Error(error)
                : Result<HIR.Instance>.Ok(document!);

        /// <summary>
        /// Runs the document at a path, statement by statement.
        /// </summary>
        public IResult<Package.ZSharp.Document, Error> Run(string path)
        {
            path = Path.GetFullPath(path);

            if (documents.TryGetValue(path, out var known))
                return Result<Package.ZSharp.Document>.Ok(known);

            if (!running.Add(path))
                return Result<Package.ZSharp.Document>.Error($"Importing {path} again while it runs is an import cycle.");

            try
            {
                if (!File.Exists(path))
                    return Result<Package.ZSharp.Document>.Error($"There is no document at {path}.");

                var document = new DocumentCompiler(language, host, path);

                using var stream = File.OpenText(path);

                var parser = new Parser.Parser(Tokenizer.Tokenizer.Tokenize(new(stream)));
                var syntax = Syntax.Install(parser);

                while (parser.HasTokens && !parser.Is(Text.TokenType.EOF))
                    if (document.Run(compiler, syntax.Document.ParseStatement(parser)) is { } error)
                        return Result<Package.ZSharp.Document>.Error(error);

                return Result<Package.ZSharp.Document>.Ok(documents[path] = document.Document);
            }
            catch (ParseError error)
            {
                return Result<Package.ZSharp.Document>.Error($"Syntax error in {path}: {error.Message}");
            }
            finally
            {
                running.Remove(path);
            }
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
