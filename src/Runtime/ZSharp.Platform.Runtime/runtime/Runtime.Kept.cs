using ZSharp.Importer.ILLoader;

namespace ZSharp.Platform.Runtime
{
    partial class Runtime
    {
        private static readonly IL.ConstructorInfo metadataAttribute
            = typeof(MetadataAttribute).GetConstructor([typeof(Type), typeof(string[]), typeof(object[])])!;

        /// <summary>
        /// Writes what a definition keeps of its annotations as the CLR platforms do, each a
        /// <c>[Metadata(type, names, values)]</c> on what the definition was built as: the
        /// annotation's type, and each of its fields by name with its value, a constant.
        /// </summary>
        private static void Keep(IEnumerable<LIR.Metadata> metadata, Action<Emit.CustomAttributeBuilder> write)
        {
            foreach (var kept in metadata.OfType<LIR.KeptAnnotation>())
                write(Written(kept.Value));
        }

        private static Emit.CustomAttributeBuilder Written(object annotation)
        {
            var fields = annotation.GetType().GetFields(IL.BindingFlags.Instance | IL.BindingFlags.Public | IL.BindingFlags.NonPublic);

            return new(metadataAttribute, [
                annotation.GetType(),
                fields.Select(field => field.Name).ToArray(),
                fields.Select(field => field.GetValue(annotation) is var value && value is string or int or long or bool
                    ? value
                    : throw new NotSupportedException($"An annotation kept as metadata, a {annotation.GetType().Name}, has state that isn't a constant.")).ToArray(),
            ]);
        }
    }
}
