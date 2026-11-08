using ZSharp.Importer.ILLoader;

namespace Core.Runtime
{
    /// <summary>
    /// The running platform as seen through <c>core:runtime</c>. The CLI sets these
    /// once it has created them.
    /// </summary>
    [ModuleScope]
    public static class ModuleScope
    {
        [Alias("runtime")]
        public static ZSharp.Platform.Runtime.Runtime Runtime { get; set; } = null!;

        [Alias("ilLoader")]
        public static ILLoader ILLoader { get; set; } = null!;

        /// <summary>
        /// Makes the first parameter the object the method runs on.
        /// </summary>
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
