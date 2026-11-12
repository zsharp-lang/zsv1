using System.Reflection;
using System.Runtime.CompilerServices;

namespace ZSharp.Importer.ILLoader
{
    /// <summary>
    /// The extension methods of the CLR classes added, as members of the values their
    /// first parameter takes. A class is added when an import reaches it, as C#'s
    /// <c>using static</c> brings a class's extension methods.
    /// </summary>
    public sealed class ExtensionMethods(ILLoader loader)
        : IExternalMembers
    {
        private readonly List<Type> classes = [];

        /// <summary>
        /// Adds the extension methods of a class, if it has any.
        /// </summary>
        public void Add(Type type)
        {
            if (type.IsDefined(typeof(ExtensionAttribute), false) && !classes.Contains(type))
                classes.Add(type);
        }

        public IEnumerable<CompilerObject> Members(CompilerObject @object, MemberName name)
        {
            if (!ZSharp.Compiler.Current.TS.TypeOf(@object).Ok(out var type) || !type.Is<IILType>(out var known))
                return [];

            var receiver = known.GetILType();

            return classes
                .SelectMany(@class => @class.GetMethods(BindingFlags.Public | BindingFlags.Static))
                .Where(method => method.Name == name
                    && method.IsDefined(typeof(ExtensionAttribute), false)
                    && method.GetParameters() is [var first, ..]
                    && Inference.Accepts(first.ParameterType, receiver))
                .Select(method => (CompilerObject)new Objects.Extension(new Objects.Method(loader, method, null), @object))
                .ToArray();
        }
    }
}
