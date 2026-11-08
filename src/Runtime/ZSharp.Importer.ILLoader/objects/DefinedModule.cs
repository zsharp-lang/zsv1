namespace ZSharp.Importer.ILLoader.Objects
{
    /// <summary>
    /// A module defined from HIR: its classes, its functions and its <c>let</c>s.
    /// </summary>
    internal sealed class DefinedModule(ILLoader loader, Package.DotNet.Module node)
        : CompilerObject
        , ICTGetMember<MemberName>
    {
        public IResult Member(MemberName member)
        {
            if (node.Definitions.OfType<Package.DotNet.Class>().FirstOrDefault(@class => @class.Name == member) is { } found)
                return Result.Ok(loader.Class(found));

            var functions = node.Methods
                .Where(method => method.Name == member)
                .Select(loader.ModuleFunction)
                .OfType<CompilerObject>()
                .ToArray();

            if (functions.Length > 0)
                return Result.Ok(OverloadGroup.Of(functions));

            if (node.Fields.FirstOrDefault(field => field.Name == member) is { } field && loader.ModuleGlobal(field) is { } global)
                return Result.Ok(global);

            return Result.Error(new UnsupportedError($"Module {node.Name} has no member named {member}."));
        }
    }
}
