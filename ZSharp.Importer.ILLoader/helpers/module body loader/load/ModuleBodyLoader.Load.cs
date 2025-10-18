namespace ZSharp.Importer.ILLoader
{
    partial class ModuleBodyLoader
        : ILoader<IL.MemberInfo>
    {
        public CompilerObject LoadMember(IL.MemberInfo member)
            => member switch
            {
                IL.FieldInfo field => LoadField(field),
                IL.MethodInfo method => LoadMethod(method),
                IL.PropertyInfo property => LoadProperty(property),
                Type type => Loader.LoadType(type),
                _ => throw new NotSupportedException(),
            };
    }
}
