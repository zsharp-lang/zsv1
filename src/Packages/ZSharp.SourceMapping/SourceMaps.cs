using System.Runtime.CompilerServices;
using ZSharp.Importer.ILLoader;

namespace ZSharp.SourceMapping
{
    /// <summary>
    /// Where each statement of some code was written. As an annotation of what holds the
    /// code, as a <see cref="HIR.Code.Code"/>, it says so of that code's statements, and
    /// goes with it: HIR knows nothing of it, so what makes the code, and what reads it,
    /// decide what it means.
    /// </summary>
    public sealed class CodeMaps
    {
        private readonly ConditionalWeakTable<HIR.Code.Statement, SourceMap> maps = new();

        public SourceMap? this[HIR.Code.Statement statement]
        {
            get => maps.TryGetValue(statement, out var map) ? map : null;
            set
            {
                if (value is null) maps.Remove(statement);
                else maps.AddOrUpdate(statement, value);
            }
        }

        /// <summary>
        /// The maps an annotated node has, if it has any.
        /// </summary>
        public static CodeMaps? Of(HIR.Annotated node)
            => node.Annotations.OfType<CodeMaps>().FirstOrDefault();

        /// <summary>
        /// The maps an annotated node has, which it is given if it has none.
        /// </summary>
        public static CodeMaps In(HIR.Annotated node)
        {
            if (Of(node) is { } maps)
                return maps;

            node.Annotations.Add(maps = new());

            return maps;
        }
    }

    /// <summary>
    /// Where each statement of a lowered definition's code was written, as metadata of the
    /// definition, as an LIR function, which a platform reads as it writes the code.
    /// </summary>
    public sealed class FunctionMaps
        : LIR.Metadata
    {
        private readonly ConditionalWeakTable<LIR.Statement, SourceMap> maps = new();

        public SourceMap? this[LIR.Statement statement]
        {
            get => maps.TryGetValue(statement, out var map) ? map : null;
            set
            {
                if (value is null) maps.Remove(statement);
                else maps.AddOrUpdate(statement, value);
            }
        }

        public static FunctionMaps? Of(LIR.Definition definition)
            => definition.Metadata.OfType<FunctionMaps>().FirstOrDefault();

        public static FunctionMaps In(LIR.Definition definition)
        {
            if (Of(definition) is { } maps)
                return maps;

            definition.Metadata.Add(maps = new());

            return maps;
        }
    }

    /// <summary>
    /// <c>sourceMap(holder, statement)</c> gives where a statement of what the holder holds
    /// was written, and <c>sourceMap(holder, statement, map)</c> sets it, for an annotated
    /// HIR node's code and a lowered LIR definition's.
    /// </summary>
    [ModuleScope]
    public static class SourceMaps
    {
        [Alias("sourceMap")]
        public static SourceMap? Of(HIR.Annotated holder, HIR.Code.Statement statement)
            => CodeMaps.Of(holder)?[statement];

        [Alias("sourceMap")]
        public static void Set(HIR.Annotated holder, HIR.Code.Statement statement, SourceMap map)
            => CodeMaps.In(holder)[statement] = map;

        [Alias("sourceMap")]
        public static SourceMap? Of(LIR.Definition holder, LIR.Statement statement)
            => FunctionMaps.Of(holder)?[statement];

        [Alias("sourceMap")]
        public static void Set(LIR.Definition holder, LIR.Statement statement, SourceMap map)
            => FunctionMaps.In(holder)[statement] = map;
    }
}
