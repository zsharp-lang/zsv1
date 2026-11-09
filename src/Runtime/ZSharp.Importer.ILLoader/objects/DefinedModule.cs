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
            // A class or an interface is found as a type, which it is while it is still
            // being loaded too.
            if (node.Definitions.OfType<Package.DotNet.TypeReference>().FirstOrDefault(type => Named(type, member)) is { } found)
                return Core.Compiler.ModuleScope.HIRLoader.LoadType(found);

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

        private static bool Named(Package.DotNet.TypeReference type, string name)
            => type switch
            {
                Package.DotNet.Class @class => @class.Name == name,
                Package.DotNet.Interface @interface => @interface.Name == name,
                _ => false
            };
    }
}
