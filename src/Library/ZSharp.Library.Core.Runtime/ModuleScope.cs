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
    }
}
