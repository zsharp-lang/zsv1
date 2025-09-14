namespace ZSharp.Importer.ILLoader
{
    partial class ModuleLoader
    {
        public CompilerObject LoadMember(IL.MemberInfo member)
            => member switch
            {
                IL.FieldInfo field => LoadField(field),
                IL.MethodInfo method => LoadMethod(method),
                IL.PropertyInfo property => LoadProperty(property),
                _ => throw new NotSupportedException(),
            };
    }
}
