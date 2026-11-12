using ZSharp.Importer.ILLoader;

namespace Core.Runtime
{
    partial class ModuleScope
    {
        /// <summary>
        /// Makes a definition of the CLR's public: anything can reach it. It applies to the CLR's
        /// definitions alone, whose accessibility the CLR has.
        /// </summary>
        [Alias("public")] public static Package.DotNet.Class Public(Package.DotNet.Class definition) => With(definition, Package.DotNet.Accessibility.Public);
        [Alias("public")] public static Package.DotNet.Interface Public(Package.DotNet.Interface definition) => With(definition, Package.DotNet.Accessibility.Public);
        [Alias("public")] public static Package.DotNet.Struct Public(Package.DotNet.Struct definition) => With(definition, Package.DotNet.Accessibility.Public);
        [Alias("public")] public static Package.DotNet.Module Public(Package.DotNet.Module definition) => With(definition, Package.DotNet.Accessibility.Public);
        [Alias("public")] public static Package.DotNet.Method Public(Package.DotNet.Method definition) => With(definition, Package.DotNet.Accessibility.Public);
        [Alias("public")] public static Package.DotNet.Constructor Public(Package.DotNet.Constructor definition) => With(definition, Package.DotNet.Accessibility.Public);
        [Alias("public")] public static Package.DotNet.Field Public(Package.DotNet.Field definition) => With(definition, Package.DotNet.Accessibility.Public);

        /// <summary>
        /// Makes a definition of the CLR's private: only what defines it can reach it; a top-level type is internal, since the CLR has no private one. It applies to the CLR's
        /// definitions alone, whose accessibility the CLR has.
        /// </summary>
        [Alias("private")] public static Package.DotNet.Class Private(Package.DotNet.Class definition) => With(definition, Package.DotNet.Accessibility.Private);
        [Alias("private")] public static Package.DotNet.Interface Private(Package.DotNet.Interface definition) => With(definition, Package.DotNet.Accessibility.Private);
        [Alias("private")] public static Package.DotNet.Struct Private(Package.DotNet.Struct definition) => With(definition, Package.DotNet.Accessibility.Private);
        [Alias("private")] public static Package.DotNet.Module Private(Package.DotNet.Module definition) => With(definition, Package.DotNet.Accessibility.Private);
        [Alias("private")] public static Package.DotNet.Method Private(Package.DotNet.Method definition) => With(definition, Package.DotNet.Accessibility.Private);
        [Alias("private")] public static Package.DotNet.Constructor Private(Package.DotNet.Constructor definition) => With(definition, Package.DotNet.Accessibility.Private);
        [Alias("private")] public static Package.DotNet.Field Private(Package.DotNet.Field definition) => With(definition, Package.DotNet.Accessibility.Private);

        /// <summary>
        /// Makes a definition of the CLR's internal: only its assembly can reach it. It applies to the CLR's
        /// definitions alone, whose accessibility the CLR has.
        /// </summary>
        [Alias("internal")] public static Package.DotNet.Class Internal(Package.DotNet.Class definition) => With(definition, Package.DotNet.Accessibility.Internal);
        [Alias("internal")] public static Package.DotNet.Interface Internal(Package.DotNet.Interface definition) => With(definition, Package.DotNet.Accessibility.Internal);
        [Alias("internal")] public static Package.DotNet.Struct Internal(Package.DotNet.Struct definition) => With(definition, Package.DotNet.Accessibility.Internal);
        [Alias("internal")] public static Package.DotNet.Module Internal(Package.DotNet.Module definition) => With(definition, Package.DotNet.Accessibility.Internal);
        [Alias("internal")] public static Package.DotNet.Method Internal(Package.DotNet.Method definition) => With(definition, Package.DotNet.Accessibility.Internal);
        [Alias("internal")] public static Package.DotNet.Constructor Internal(Package.DotNet.Constructor definition) => With(definition, Package.DotNet.Accessibility.Internal);
        [Alias("internal")] public static Package.DotNet.Field Internal(Package.DotNet.Field definition) => With(definition, Package.DotNet.Accessibility.Internal);

        /// <summary>
        /// Makes a definition of the CLR's protected: what defines it, and what derives from it, can reach it. It applies to the CLR's
        /// definitions alone, whose accessibility the CLR has.
        /// </summary>
        [Alias("protected")] public static Package.DotNet.Class Protected(Package.DotNet.Class definition) => With(definition, Package.DotNet.Accessibility.Protected);
        [Alias("protected")] public static Package.DotNet.Interface Protected(Package.DotNet.Interface definition) => With(definition, Package.DotNet.Accessibility.Protected);
        [Alias("protected")] public static Package.DotNet.Struct Protected(Package.DotNet.Struct definition) => With(definition, Package.DotNet.Accessibility.Protected);
        [Alias("protected")] public static Package.DotNet.Module Protected(Package.DotNet.Module definition) => With(definition, Package.DotNet.Accessibility.Protected);
        [Alias("protected")] public static Package.DotNet.Method Protected(Package.DotNet.Method definition) => With(definition, Package.DotNet.Accessibility.Protected);
        [Alias("protected")] public static Package.DotNet.Constructor Protected(Package.DotNet.Constructor definition) => With(definition, Package.DotNet.Accessibility.Protected);
        [Alias("protected")] public static Package.DotNet.Field Protected(Package.DotNet.Field definition) => With(definition, Package.DotNet.Accessibility.Protected);

        private static T With<T>(T definition, Package.DotNet.Accessibility accessibility)
            where T : Package.DotNet.Accessible
        {
            definition.Accessibility = accessibility;
            return definition;
        }
    }
}
