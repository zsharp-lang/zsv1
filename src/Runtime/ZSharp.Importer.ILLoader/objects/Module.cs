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
            // Types imported under one name, such as the arities of a generic family, are
            // its overloads: giving arguments picks the one they fit.
            var imported = il
                .GetCustomAttributes<ImportTypeAttribute>()
                .Where(attribute => (attribute.Alias ?? attribute.Type.Name) == member)
                .Select(attribute => Imported(attribute.Type))
                .ToArray();

            if (imported.Length > 0)
                return Result.Ok(OverloadGroup.Of(imported));

            var types = il.GetTypes();

            foreach (var scope in types.Where(type => type.IsDefined(typeof(ModuleScopeAttribute), false)))
            {
                // A static readonly field of the module scope that holds HIR is what the
                // module exports under its name: the HIR itself, not a value of the CLR.
                var exported = scope
                    .GetFields(BindingFlags.Public | BindingFlags.Static)
                    .FirstOrDefault(field => field.IsInitOnly
                        && typeof(HIR.Instance).IsAssignableFrom(field.FieldType)
                        && (field.GetCustomAttribute<AliasAttribute>()?.Name ?? field.Name) == member);

                if (exported?.GetValue(null) is HIR.Instance instance)
                    return Core.Compiler.ModuleScope.HIRLoader.LoadInstance(instance);

                var result = loader.Class(scope).Member(member);

                if (!result.IsUnsupported())
                    return result;
            }

            if (types.FirstOrDefault(type => type.IsPublic && type.Name == member) is { } found)
                return Result.Ok(Imported(found));

            return Result.Error(new UnsupportedError($"Module {il.Name} has no member named {member}."));
        }

        /// <summary>
        /// A type an import reaches. Its extension methods come with it.
        /// </summary>
        private CompilerObject Imported(Type type)
        {
            loader.Extensions.Add(type);

            return loader.Class(type);
        }
    }
}
