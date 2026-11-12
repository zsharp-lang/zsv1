using ZSharp.Importer.ILLoader;

namespace Package.DotNet
{
    /// <summary>
    /// The decorators of the CLR's definitions. Each sets what the CLR has of a definition
    /// on its HIR and gives the definition back, as it was given. They are HIR's alone and
    /// need nothing that runs.
    /// </summary>
    [ModuleScope]
    public static class Decorators
    {
        /// <summary>
        /// Makes a definition public: anything can reach it.
        /// </summary>
        [Alias("public")]
        public static T Public<T>(T definition) where T : Accessible
            => With(definition, Accessibility.Public);

        /// <summary>
        /// Makes a definition private: only what defines it can reach it. A top-level type
        /// is internal, since the CLR has no private one.
        /// </summary>
        [Alias("private")]
        public static T Private<T>(T definition) where T : Accessible
            => With(definition, Accessibility.Private);

        /// <summary>
        /// Makes a definition internal: only its assembly can reach it.
        /// </summary>
        [Alias("internal")]
        public static T Internal<T>(T definition) where T : Accessible
            => With(definition, Accessibility.Internal);

        /// <summary>
        /// Makes a definition protected: what defines it, and what derives from it, can
        /// reach it.
        /// </summary>
        [Alias("protected")]
        public static T Protected<T>(T definition) where T : Accessible
            => With(definition, Accessibility.Protected);

        /// <summary>
        /// Makes a method one a derived class can override.
        /// </summary>
        [Alias("virtual")]
        public static Method Virtual(Method method)
        {
            method.IsVirtual = true;
            return method;
        }

        /// <summary>
        /// Makes a method one with no code of its own, which a derived class must override.
        /// </summary>
        [Alias("abstract")]
        public static Method Abstract(Method method)
        {
            method.IsAbstract = true;
            return method;
        }

        /// <summary>
        /// Makes a method override the virtual method of the base class it shares a
        /// signature with.
        /// </summary>
        [Alias("override")]
        public static Method Override(Method method)
        {
            method.IsOverride = true;
            return method;
        }

        /// <summary>
        /// Makes a method one of its type, not of an object of it.
        /// </summary>
        [Alias("static")]
        public static Method Static(Method method)
        {
            method.IsStatic = true;
            return method;
        }

        /// <summary>
        /// Makes the first parameter of a method the object it runs on.
        /// </summary>
        [Alias("explicitThis")]
        public static Method ExplicitThis(Method method)
        {
            method.HasExplicitThis = true;
            return method;
        }

        /// <summary>
        /// Makes the first parameter of a constructor the object it runs on.
        /// </summary>
        [Alias("explicitThis")]
        public static Constructor ExplicitThis(Constructor constructor)
        {
            constructor.HasExplicitThis = true;
            return constructor;
        }

        private static T With<T>(T definition, Accessibility accessibility)
            where T : Accessible
        {
            definition.Accessibility = accessibility;
            return definition;
        }
    }
}
