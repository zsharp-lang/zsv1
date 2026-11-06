namespace ZSharp.Importer.ILLoader.Objects
{
    /// <summary>
    /// A module that was defined and has been loaded.
    /// </summary>
    internal sealed class DefinedModule(ILLoader loader, Package.DotNet.Module node)
        : CompilerObject
        , ICTGetMember<MemberName>
    {
        public IResult Member(MemberName member)
            => node.Definitions.OfType<Package.DotNet.Class>().FirstOrDefault(@class => @class.Name == member) is { } found
                ? Result.Ok(loader.Class(found))
                : Result.Error(new UnsupportedError($"Module {node.Name} has no member named {member}."));
    }
}
