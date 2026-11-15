using System.Runtime.CompilerServices;
using ZSharp.Importer.ILLoader;

namespace ZSharp.SourceMapping
{
    /// <summary>
    /// Where each node came from, as <c>sourceMap(node)</c> gives it and
    /// <c>sourceMap(node, map)</c> sets it, for each kind of node the chain carries. How a
    /// kind keeps its maps is its own: each here is a table beside the nodes, keyed by
    /// them, so a node holds nothing of it, and a map lives as long as its node does.
    /// </summary>
    [ModuleScope]
    public static class SourceMaps
    {
        private static readonly ConditionalWeakTable<HIR.Identity, SourceMap> identities = new();

        private static readonly ConditionalWeakTable<HIR.Code.Statement, SourceMap> statements = new();

        private static readonly ConditionalWeakTable<HIR.Code.Expression, SourceMap> expressions = new();

        private static readonly ConditionalWeakTable<LIR.Statement, SourceMap> lowered = new();

        [Alias("sourceMap")]
        public static SourceMap? Of(HIR.Identity identity)
            => identities.TryGetValue(identity, out var map) ? map : null;

        [Alias("sourceMap")]
        public static void Set(HIR.Identity identity, SourceMap map)
            => identities.AddOrUpdate(identity, map);

        [Alias("sourceMap")]
        public static SourceMap? Of(HIR.Code.Statement statement)
            => statements.TryGetValue(statement, out var map) ? map : null;

        [Alias("sourceMap")]
        public static void Set(HIR.Code.Statement statement, SourceMap map)
            => statements.AddOrUpdate(statement, map);

        [Alias("sourceMap")]
        public static SourceMap? Of(HIR.Code.Expression expression)
            => expressions.TryGetValue(expression, out var map) ? map : null;

        [Alias("sourceMap")]
        public static void Set(HIR.Code.Expression expression, SourceMap map)
            => expressions.AddOrUpdate(expression, map);

        [Alias("sourceMap")]
        public static SourceMap? Of(LIR.Statement statement)
            => lowered.TryGetValue(statement, out var map) ? map : null;

        [Alias("sourceMap")]
        public static void Set(LIR.Statement statement, SourceMap map)
            => lowered.AddOrUpdate(statement, map);
    }
}
