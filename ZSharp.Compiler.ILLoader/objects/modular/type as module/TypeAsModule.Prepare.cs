namespace ZSharp.Compiler.ILLoader.Objects
{
    partial class TypeAsModule
    {
        private void Prepare()
        {
            foreach (var member in IL.GetMembers())
                Prepare(member);
        }

        private void Prepare(IL.MemberInfo member)
        {
            if (member.DeclaringType != IL)
                return;

            if (member is Type type && type.IsModuleScope())
                throw new InvalidOperationException($"Module scope types are not allowed: {type.FullName}");

            if (member is IL.MethodInfo method && method.IsOperator())
            {
                Console.WriteLine($"Skipping operator method: {method.Name}");
                return;
            }

            var @namespace = member.GetNamespaceOverride(RootNamespace);
            ILazilyLoadMembers lazyLoader = @namespace == string.Empty
                ? BodyLoader
                : Loader.Namespace(@namespace);

            lazyLoader.AddLazyMember(member.AliasOrName(), member);
        }
    }
}
