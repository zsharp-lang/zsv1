namespace ZSharp.Compiler
{
    /// <summary>
    /// Loads a document as an object whose members are what it exports.
    /// </summary>
    public static class DocumentLoaders
    {
        public static void Install(MappedLoader loader, HIRLoader hir)
            => loader.Instances.Register<Package.ZSharp.Document>(
                document => Result<MIR.CompilerObject>.Ok(new DocumentObject(document, hir))
            );

        private sealed class DocumentObject(Package.ZSharp.Document document, HIRLoader loader)
            : MIR.CompilerObject
            , ICTGetMember<string>
        {
            public IResult<MIR.CompilerObject, Error> Member(string member)
                => document.Exports.TryGetValue(member, out var exported)
                    ? loader.LoadInstance(exported)
                    : Result<MIR.CompilerObject>.Error(new UnsupportedError($"Document {document.Path} exports nothing named {member}."));
        }
    }
}
