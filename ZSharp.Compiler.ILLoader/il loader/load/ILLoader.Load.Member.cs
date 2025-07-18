namespace ZSharp.Compiler.ILLoader
{
    partial class ILLoader
    {
        private readonly Dictionary<IL.MemberInfo, CompilerObject> memberCache = [];

        public CompilerObject LoadMember(IL.MemberInfo member)
        {
            if (!memberCache.TryGetValue(member, out var @object))
                @object = memberCache[member] = member switch
                {
                    IL.ConstructorInfo constructor => LoadConstructor(constructor),
                    IL.EventInfo @event => LoadEvent(@event),
                    IL.FieldInfo field => LoadField(field),
                    IL.MethodInfo method => LoadMethod(method),
                    IL.PropertyInfo property => LoadProperty(property),
                    _ => throw new NotSupportedException()
                };

            return @object;
        }
    }
}
