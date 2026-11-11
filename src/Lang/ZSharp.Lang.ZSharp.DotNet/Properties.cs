namespace ZSharp.Lang.DotNet
{
    /// <summary>
    /// A CLR class's properties, known once its accessors are built: a property is the
    /// methods its <c>get</c> and <c>set</c> accessors are, as the CLR has it, named for it.
    /// It is built as its accessors are written; what a property needs of them is checked
    /// as the class is loaded.
    /// </summary>
    public static class Properties
    {
        public static string AccessorName(AST.Accessor accessor)
            => (accessor.IsSetter ? "set_" : "get_") + accessor.Name;

        public static void Define(Package.DotNet.Class owner, string name)
        {
            var getter = owner.Methods.FirstOrDefault(method => method.Name == "get_" + name);
            var setter = owner.Methods.FirstOrDefault(method => method.Name == "set_" + name);

            owner.Properties.Add(new(name, getter?.ReturnType ?? setter?.Parameters.LastOrDefault()?.Type)
            {
                Getter = getter,
                Setter = setter,
                IsStatic = (getter ?? setter)?.IsStatic ?? false,
            });
        }
    }
}
