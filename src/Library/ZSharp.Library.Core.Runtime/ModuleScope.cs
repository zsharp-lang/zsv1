using ZSharp.Importer.ILLoader;

namespace Core.Runtime
{
    /// <summary>
    /// The running platform as seen through <c>core:runtime</c>. The CLI sets these
    /// once it has created them.
    /// </summary>
    [ModuleScope]
    public static partial class ModuleScope
    {
        [Alias("runtime")]
        public static ZSharp.Platform.Runtime.Runtime Runtime { get; set; } = null!;

        [Alias("ilLoader")]
        public static ILLoader ILLoader { get; set; } = null!;

        /// <summary>
        /// Makes the first parameter the object the method runs on.
        /// </summary>
        /// <summary>
        /// Makes a method one a derived class can override.
        /// </summary>
        [Alias("virtual")]
        public static Package.DotNet.Method Virtual(Package.DotNet.Method method)
        {
            method.IsVirtual = true;
            return method;
        }

        /// <summary>
        /// Makes a method one with no code of its own, which a derived class must override.
        /// </summary>
        [Alias("abstract")]
        public static Package.DotNet.Method Abstract(Package.DotNet.Method method)
        {
            method.IsAbstract = true;
            return method;
        }

        /// <summary>
        /// Makes a method override the virtual method of the base class it shares a
        /// signature with.
        /// </summary>
        [Alias("override")]
        public static Package.DotNet.Method Override(Package.DotNet.Method method)
        {
            method.IsOverride = true;
            return method;
        }

        [Alias("explicitThis")]
        public static Package.DotNet.Method ExplicitThis(Package.DotNet.Method method)
        {
            method.CustomMetadata.Add(ExplicitThis());
            return method;
        }

        /// <summary>
        /// Makes the first parameter the object the constructor runs on.
        /// </summary>
        [Alias("explicitThis")]
        public static Package.DotNet.Constructor ExplicitThis(Package.DotNet.Constructor constructor)
        {
            constructor.CustomMetadata.Add(ExplicitThis());
            return constructor;
        }

        [Alias("static")]
        public static Package.DotNet.Method Static(Package.DotNet.Method method)
        {
            method.IsStatic = true;
            return method;
        }

        private static Package.DotNet.CustomMetadata ExplicitThis()
            => new(ILLoader.Load(typeof(ExplicitThisAttribute).GetConstructor(Type.EmptyTypes)!));
    }
}
