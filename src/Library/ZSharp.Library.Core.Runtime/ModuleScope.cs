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
        /// The type of a reference to a variable, as <c>Ref[Int32]</c>, which a <c>ref</c>
        /// parameter takes.
        /// </summary>
        [Alias("Ref")]
        public static readonly ZSharp.HIR.Instance RefFamily = new Package.DotNet.RefFamily(isOut: false);

        /// <summary>
        /// The type of a reference to a variable the callee only assigns, as
        /// <c>Out[Int32]</c>, which an <c>out</c> parameter takes.
        /// </summary>
        [Alias("Out")]
        public static readonly ZSharp.HIR.Instance OutFamily = new Package.DotNet.RefFamily(isOut: true);

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

        private static Package.DotNet.CustomMetadata ExplicitThis()
            => new(ILLoader.Load(typeof(ExplicitThisAttribute).GetConstructor(Type.EmptyTypes)!));
    }
}
