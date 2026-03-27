namespace ZSharp.SourceCompiler
{
    partial class DefinitionCompiler
    {
        private readonly Dictionary<Type, CompilerObject> defaultMetaTypes = [];

        public void DefaultMetaType<T>(CompilerObject metaType)
        {
            defaultMetaTypes[typeof(T)] = metaType;
        }

        private CompilerObject? DefaultMetaType<T>() => DefaultMetaType(typeof(T));

        private CompilerObject? DefaultMetaType(Type type)
        {
            if (defaultMetaTypes.TryGetValue(type, out var metaType))
                return metaType;
            else return null;
        }
    }
}
