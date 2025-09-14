using System.Reflection;

namespace ZSharp.Importer.ILLoader.Objects
{
    internal sealed partial class Module
        : CompilerObject
    {
        public string Name => IL.Name;

        public Module(IL.Module il, ILLoader loader)
        {
            IL = il;
            Loader = loader;
            BodyLoader = new(this, loader);

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

                    ILazilyLoadMembers lazyLoader = ns == string.Empty
                        ? BodyLoader
                        : Loader.Namespace(ns);

                    lazyLoader.AddLazyMember(type.AliasOrName(), type);
                }

            PrepareGlobals(moduleScopes);
        }
    }
}
