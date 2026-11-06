using System.Reflection;

namespace ZSharp.Importer.ILLoader.Objects
{
    /// <summary>
    /// A CLR module. Its members are the types it imports, the static members of its
    /// module scope and its public types, found when asked.
    /// </summary>
    internal sealed class Module(ILLoader loader, System.Reflection.Module il)
        : CompilerObject
        , ICTGetMember<MemberName>
    {
        public IResult Member(MemberName member)
        {
            var imported = il
                .GetCustomAttributes<ImportTypeAttribute>()
                .FirstOrDefault(attribute => (attribute.Alias ?? attribute.Type.Name) == member);

            if (imported is not null)
                return Result.Ok(loader.Class(imported.Type));

            var types = il.GetTypes();

            foreach (var scope in types.Where(type => type.IsDefined(typeof(ModuleScopeAttribute), false)))
            {
                var result = loader.Class(scope).Member(member);

                if (!result.IsUnsupported())
                    return result;
            }

            if (types.FirstOrDefault(type => type.IsPublic && type.Name == member) is { } found)
                return Result.Ok(loader.Class(found));

            return Result.Error(new UnsupportedError($"Module {il.Name} has no member named {member}."));
        }
    }
}
