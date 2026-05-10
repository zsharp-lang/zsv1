namespace Core.Runtime
{
    partial class ModuleScope
    {
        [Alias("runtime")]
        public static ZSharp.Platform.Runtime.Runtime Runtime { get; set; } = null!;

        [Alias("ilLoader")]
        public static ILLoader ILLoader { get; set; } = null!;

        [Alias("rtLoader")]
        public static ZSharp.Importer.RT.Loader RTLoader { get; set; } = null!;
    }
}
