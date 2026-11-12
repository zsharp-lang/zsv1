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
        /// What imports an assembly of the running platform as its module, as importing
        /// its file does. The host sets it, with what imports a <c>.dll</c>.
        /// </summary>
        public static Assemblies Assemblies { get; set; } = null!;

        /// <summary>
        /// Imports an assembly the running platform has, as its module, so what reaches the
        /// assembly needn't find its file: <c>import { BindingFlags } from Object.Assembly</c>.
        /// </summary>
        [Operator("import")]
        public static ZSharp.HIR.Instance Import(System.Reflection.Assembly assembly)
            => Assemblies.Import(assembly);
    }
}
