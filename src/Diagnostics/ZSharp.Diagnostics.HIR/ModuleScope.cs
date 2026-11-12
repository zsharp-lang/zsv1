using ZSharp.Importer.ILLoader;

namespace ZSharp.Diagnostics
{
    /// <summary>
    /// Diagnostics as seen through <c>core:diagnostics</c>.
    /// </summary>
    [ModuleScope]
    public static class ModuleScope
    {
        /// <summary>
        /// Prints an HIR object to standard error, and returns it.
        /// </summary>
        [Alias("dump")]
        public static object Dump(object node)
        {
            Console.Error.WriteLine(HIRPrinter.Print(node));
            return node;
        }
    }
}
