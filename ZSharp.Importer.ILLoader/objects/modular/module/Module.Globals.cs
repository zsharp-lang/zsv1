namespace ZSharp.Importer.ILLoader.Objects
{
    partial class Module
    {
        private void PrepareGlobals(IEnumerable<Type> scopes)
        {
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
    }
}
