namespace ZSharp.Lang.DotNet
{
    /// <summary>
    /// A CLR class's properties, known once its accessors are built: a property is the
    /// methods its <c>get</c> and <c>set</c> accessors are. An accessor is named for what it
    /// is, <c>get Name</c> or <c>set Name</c>; the platform names it its own way, as the CLR
    /// names it <c>get_Name</c>. What a property needs of them is checked as the class is
    /// loaded.
    /// </summary>
    public static class Properties
    {
        public static string AccessorName(AST.Accessor accessor)
            => (accessor.Kind == AST.AccessorKind.Set ? "set " : "get ") + accessor.Name;

        public static void Define(Package.DotNet.Class owner, string name)
        {
            var getter = owner.Methods.FirstOrDefault(method => method.Name == "get " + name);
            var setter = owner.Methods.FirstOrDefault(method => method.Name == "set " + name);

            owner.Properties.Add(new(name, getter?.ReturnType ?? setter?.Parameters.LastOrDefault()?.Type)
            {
                Getter = getter,
                Setter = setter,
                IsStatic = (getter ?? setter)?.IsStatic ?? false,
            });
        }
    }
}
