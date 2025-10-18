using System.Reflection;

namespace ZSharp.Importer.ILLoader
{
    internal static class Prepare
    {
        public static void PrepareModule(Objects.Module module, ILLoader loader)
        {
            var il = module.IL;

            Dictionary<string, string> mappedNamespaces = [];
            foreach (var mapping in il.GetCustomAttributes<MapNamespaceAttribute>())
                mappedNamespaces[mapping.OldName] = mapping.NewName;

            List<Type> moduleScopes = [];

            foreach (var type in il.GetTypes())
                if (!type.IsPublic) continue;
                else
                {
                    if (type.Namespace is string ns)
                        ns = mappedNamespaces.GetValueOrDefault(ns, ns);
                    else ns = string.Empty;

                    if (type.IsModuleScope())
                    {
                        moduleScopes.Add(type);
                        continue;
                    }

                    ILazyMemberLoader lazyLoader = ns == string.Empty
                        ? module.LazyLoader
                        : loader.Namespace(ns);

                    lazyLoader.AddLazyMember(type.AliasOrName(), type);
                }

            foreach (var importedType in il.GetImportedTypes())
                PrepareImportedType(importedType, module.LazyLoader, loader);
            foreach (var scope in moduleScopes)
                PrepareModuleScope(scope, module.LazyLoader, loader);
        }

        public static void PrepareTypeAsModule(Objects.TypeAsModule module, ILLoader loader)
        {
            var il = module.IL;

            Dictionary<string, string> mappedNamespaces = [];
            foreach (var mapping in il.GetCustomAttributes<MapNamespaceAttribute>())
                mappedNamespaces[mapping.OldName] = mapping.NewName;

            List<Type> moduleScopes = [];

            foreach (var type in il.GetNestedTypes())
                if (!type.IsPublic) continue;
                else
                {
                    if (type.Namespace is string ns)
                        ns = mappedNamespaces.GetValueOrDefault(ns, ns);
                    else ns = string.Empty;

                    if (type.IsModuleScope())
                    {
                        moduleScopes.Add(type);
                        continue;
                    }

                    ILazyMemberLoader lazyLoader = ns == string.Empty
                        ? module.LazyLoader
                        : loader.Namespace(ns);

                    lazyLoader.AddLazyMember(type.AliasOrName(), type);
                }

            foreach (var importedType in il.GetImportedTypes())
                PrepareImportedType(importedType, module.LazyLoader, loader);

            PrepareModuleScope(module.IL, module.LazyLoader, loader);
        }

        private static void PrepareModuleScope(Type scope, ILazyMemberLoader lazyLoader, ILLoader loader)
        {
            foreach (var member in scope.GetMembers())
            {
                if (member.DeclaringType != scope) continue;

                if (member is MethodInfo method && method.IsOperator(out var op))
                {
                    loader.OnLoadOperator?.Invoke(op, method);
                    continue;
                }

                lazyLoader.AddLazyMember(member.AliasOrName(), member);
            }
        }

        private static void PrepareImportedType(ImportTypeAttribute importedType, ILazyMemberLoader lazyLoader, ILLoader loader)
        {
            var type = importedType.Type;
            if (!type.IsPublic) throw new($"Cannot use {nameof(ImportTypeAttribute)} on non-public type {type.Name}");
            string ns = importedType.Namespace ?? type.Namespace ?? string.Empty;
            lazyLoader = ns == string.Empty
                ? lazyLoader
                : loader.Namespace(ns);
            lazyLoader.AddLazyMember(importedType.Alias ?? type.Name, type);
        }

        public static void PrepareType(Objects.Class @class, ILLoader loader)
        {
            var il = @class.IL;

            foreach (var member in il.GetMembers())
            {
                if (member is MethodInfo method && method.IsOperator(out var op))
                {
                    loader.OnLoadOperator?.Invoke(op, method);
                    continue;
                }

                @class.LazyLoader.AddLazyMember(member.AliasOrName(), member);
            }
        }
    }
}
