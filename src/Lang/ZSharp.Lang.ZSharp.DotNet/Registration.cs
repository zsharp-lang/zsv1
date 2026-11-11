namespace ZSharp.Lang.DotNet
{
    /// <summary>
    /// How a container registers what a definition's decorators gave. A definition goes in
    /// its list; code, as <c>ct</c> gives for a definition it loaded, is emitted directly,
    /// so it registers nothing. Overloading chooses by what the decorators gave.
    /// </summary>
    public static class Registration
    {
        public static void Register(List<Package.DotNet.DotNetDefinition> list, Package.DotNet.DotNetDefinition definition)
            => list.Add(definition);

        public static void Register(List<Package.DotNet.Method> list, Package.DotNet.Method method)
            => list.Add(method);

        public static void Register(List<Package.DotNet.Constructor> list, Package.DotNet.Constructor constructor)
            => list.Add(constructor);

        /// <summary>
        /// Code is emitted where the definition was, and in the code of a container that
        /// has no effect, so nothing is registered.
        /// </summary>
        public static void Register(object list, HIR.Code.Expression code)
        {
        }
    }
}
