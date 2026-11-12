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
        , ICTCastTo
    {
        /// <summary>
        /// Casting a module to what describes it is reflection, as for a type.
        /// </summary>
        public IResult Cast(CompilerObject targetType)
            => ZSharp.Compiler.Current.TS.IsAssignableTo(targetType, loader.Class(loader.Load(il).GetType()))
                ? Result.Ok(new Literal(loader.Load(il), loader.Load(il).GetType(), loader))
                : Result.Error(new UnsupportedError("A module can't be cast to that type."));

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
                    return ZSharp.Compiler.Current.HIRLoader.LoadInstance(instance);

                var result = loader.Class(scope).Member(member);

                if (!result.IsUnsupported())
                    return result;
            }

            // A public type is found by its name, and a generic one by its name without its
            // arity. A name with a type that isn't generic is that type, which indexing with
            // type arguments makes the one of that name and arity, as C# reads `IList` and
            // `IList<T>`; a name of generic types only is its arities, as overloads.
            var found = types.Where(type => type.IsPublic && (type.Name.Split('`')[0]) == member).ToArray();

            if (found.FirstOrDefault(type => !type.IsGenericTypeDefinition) is { } plain)
            {
                foreach (var generic in found.Where(type => type != plain))
                    loader.Extensions.Add(generic);

                return Result.Ok(Imported(plain));
            }

            if (found.Length > 0)
                return Result.Ok(OverloadGroup.Of(found.Select(Imported)));

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
