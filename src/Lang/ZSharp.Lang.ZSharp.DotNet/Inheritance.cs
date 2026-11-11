namespace ZSharp.Lang.DotNet
{
    /// <summary>
    /// What a type definition's bases are, known once they are built: an interface is one
    /// it implements, and anything else is its base class, of which it has one.
    /// </summary>
    public static class Inheritance
    {
        public static void Inherit(Package.DotNet.Class @class, Package.DotNet.TypeReference @base)
        {
            if (IsInterface(@base))
                @class.Interfaces.Add(@base);
            else if (@class.Base is null && @base is Package.DotNet.ClassReference reference)
                @class.Base = reference;
            else
                throw new InvalidOperationException($"Class {@class.Name} can only have one base class.");
        }

        /// <summary>
        /// An interface only extends interfaces.
        /// </summary>
        public static void Inherit(Package.DotNet.Interface @interface, Package.DotNet.TypeReference @base)
        {
            if (!IsInterface(@base))
                throw new InvalidOperationException($"Interface {@interface.Name} can only extend interfaces.");

            @interface.Interfaces.Add(@base);
        }

        private static bool IsInterface(Package.DotNet.TypeReference type)
            => type is Package.DotNet.InterfaceReference
            || type is Package.DotNet.GenericInstance { Definition: Package.DotNet.InterfaceReference };
    }
}
