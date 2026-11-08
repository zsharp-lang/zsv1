namespace ZSharp.Importer.String
{
    /// <summary>
    /// Libraries written in Z#, as documents in a directory: <c>name</c> is
    /// <c>name.zs</c>, or else <c>name/name.zs</c>, which the given importer imports.
    /// </summary>
    public sealed class DocumentLibrary(string root, IStringImporter documents)
        : IStringImporter
    {
        public IResult<HIR.Instance, Error> Import(string source)
        {
            foreach (var path in new[] { Path.Combine(root, source + ".zs"), Path.Combine(root, source, Path.GetFileName(source) + ".zs") })
                if (File.Exists(path))
                    return documents.Import(Path.GetFullPath(path).Replace("\\", "/"));

            return Result<HIR.Instance>.Error($"Could not find the library '{source}' in {root}.");
        }

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
