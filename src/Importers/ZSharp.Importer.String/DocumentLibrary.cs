namespace ZSharp.Importer.String
{
    /// <summary>
    /// Libraries written in Z#, in a directory. A library is built, as
    /// <c>name/build/name.dll</c>, which the given assemblies import, or else a
    /// document: <c>name.zs</c>, or else <c>name/name.zs</c>, which the given importer
    /// imports. A built library's assembly is named for it, as <c>std.zsharp.lowering</c>
    /// for <c>std:zsharp/lowering</c>, by the prefix the library is imported with.
    /// </summary>
    public sealed class DocumentLibrary(string root, IStringImporter documents, string prefix, Func<string, IResult<HIR.Instance, Error>> assemblies)
        : IStringImporter
    {
        private readonly Dictionary<string, IResult<HIR.Instance, Error>> imported = [];

        /// <summary>
        /// A library, as it was the first time it was imported: one built while the process
        /// runs, as a bootstrap builds the libraries it loads, is what the next one loads.
        /// </summary>
        public IResult<HIR.Instance, Error> Import(string source)
        {
            if (!imported.TryGetValue(source, out var result))
                imported[source] = result = Find(source);

            return result;
        }

        private IResult<HIR.Instance, Error> Find(string source)
        {
            if (Built(source) is { } built)
                return assemblies(built);

            foreach (var path in new[] { Path.Combine(root, source + ".zs"), Path.Combine(root, source, Path.GetFileName(source) + ".zs") })
                if (File.Exists(path))
                    return documents.Import(Path.GetFullPath(path).Replace("\\", "/"));

            return Result<HIR.Instance>.Error($"Could not find the library '{source}' in {root}.");
        }

        /// <summary>
        /// The file of a library's built assembly, if it is built.
        /// </summary>
        public string? Built(string source)
            => Path.Combine(root, source, "build", Path.GetFileName(source) + ".dll") is var path && File.Exists(path)
                ? Path.GetFullPath(path)
                : null;

        /// <summary>
        /// The file of the built library an assembly name names, as one built library's
        /// assembly refers to another's, if it is one of these.
        /// </summary>
        public string? Assembly(string name)
            => name.StartsWith(prefix + ".") ? Built(name[(prefix.Length + 1)..].Replace('.', '/')) : null;

        /// <summary>
        /// The directory of a name found in a directory above another, as the standard
        /// library is found from where the toolchain runs, or <see langword="null"/>.
        /// </summary>
        public static string? Above(string start, string name)
        {
            for (var directory = new DirectoryInfo(start); directory is not null; directory = directory.Parent)
                if (Directory.Exists(Path.Combine(directory.FullName, name)))
                    return Path.Combine(directory.FullName, name);

            return null;
        }
    }
}
