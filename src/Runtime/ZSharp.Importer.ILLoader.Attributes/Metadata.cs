using System.Reflection;
using System.Runtime.CompilerServices;

namespace ZSharp.Importer.ILLoader
{
    /// <summary>
    /// The CLR platforms' half of reflection: an object that carries metadata gives what
    /// was kept on it, and the unit it was compiled into. A member, a type or a module
    /// carries its own; any other object carries its runtime type's, as a Z# class's type
    /// object carries its metaclass's.
    /// </summary>
    public static class Metadata
    {
        /// <summary>
        /// The CLR entity that carries an object's metadata.
        /// </summary>
        public static ICustomAttributeProvider Entity(object value)
            => value as ICustomAttributeProvider ?? value.GetType();

        /// <summary>
        /// The unit an entity was compiled into, which carries what was encoded of it.
        /// </summary>
        public static Module? Unit(ICustomAttributeProvider entity)
            => entity switch
            {
                Module module => module,
                Assembly assembly => assembly.ManifestModule,
                MemberInfo member => member.Module,
                _ => null,
            };

        /// <summary>
        /// The annotations kept on an entity, made again from what was written of them.
        /// </summary>
        public static List<object> Of(ICustomAttributeProvider entity)
        {
            var found = new List<object>();

            foreach (var attribute in entity.GetCustomAttributes(typeof(MetadataAttribute), false).Cast<MetadataAttribute>())
            {
                var annotation = RuntimeHelpers.GetUninitializedObject(attribute.Type);

                for (var index = 0; index < attribute.Names.Length; index++)
                    attribute.Type.GetField(attribute.Names[index], BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                        ?.SetValue(annotation, attribute.Values[index]);

                found.Add(annotation);
            }

            return found;
        }

        /// <summary>
        /// The source of what decodes an assembly's reflection blob, if it says.
        /// </summary>
        public static string? Decoder(Assembly assembly)
            => assembly.GetCustomAttribute<ReflectionBlobAttribute>()?.Decoder;

        /// <summary>
        /// The bytes an assembly carries as its reflection blob, or <see langword="null"/>.
        /// </summary>
        public static byte[]? Blob(Assembly assembly)
        {
            if (assembly.GetCustomAttribute<ReflectionBlobAttribute>() is not { } blob
                || assembly.GetManifestResourceStream(blob.Resource) is not { } stream)
                return null;

            using (stream)
            {
                using var bytes = new MemoryStream();
                stream.CopyTo(bytes);
                return bytes.ToArray();
            }
        }
    }
}
