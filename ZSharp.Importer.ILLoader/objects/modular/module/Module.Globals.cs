namespace ZSharp.Importer.ILLoader.Objects
{
    partial class Module
    {
        private void PrepareGlobals(IEnumerable<Type> scopes)
        {
            foreach (var importedType in IL.GetImportedTypes())
                PrepareImportedType(importedType);
            foreach (var scope in scopes)
                PrepareGlobals(scope);
        }

        private void PrepareGlobals(Type scope)
        {
            foreach (var member in scope.GetMembers())
            {
                if (member.DeclaringType != scope) continue;

                if (member is IL.MethodInfo method && method.IsOperator(out var op))
                {
                    Loader.OnLoadOperator?.Invoke(op, method);
                    continue;
                }

                BodyLoader.AddMember(member);
            }
        }

        private void PrepareImportedType(ImportTypeAttribute importedType)
        {
            var type = importedType.Type;
            if (!type.IsPublic) throw new($"Cannot use {nameof(ImportTypeAttribute)} on non-public type {type.Name}");
            string ns = importedType.Namespace ?? type.Namespace ?? string.Empty;
            ILazilyLoadMembers lazyLoader = ns == string.Empty
                ? BodyLoader
                : Loader.Namespace(ns);
            lazyLoader.AddLazyMember(importedType.Alias ?? type.Name, type);
        }
    }
}
