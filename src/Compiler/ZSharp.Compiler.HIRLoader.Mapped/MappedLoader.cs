namespace ZSharp.Compiler
{
    /// <summary>
    /// Implements <see cref="HIRLoader"/> through registered loaders. A loaded
    /// definition is cached, so one definition gives one CO.
    /// </summary>
    public sealed class MappedLoader(HIRLoader loader)
    {
        private readonly HIRLoader loader = loader;
        private readonly Dictionary<HIR.Definition, CompilerObject> definitions
            = new(ReferenceEqualityComparer.Instance);

        private HIRLoader @base = null!;

        public LoaderMap<HIR.Definition> Definitions { get; } = new();

        public LoaderMap<HIR.Instance> Instances { get; } = new();

        public LoaderMap<HIR.Type> Types { get; } = new();

        public void Apply()
        {
            @base = loader.Clone();

            loader.LoadDefinition = LoadDefinition;
            loader.LoadInstance = LoadInstance;
            loader.LoadType = LoadType;
        }

        public IResult LoadDefinition(HIR.Definition definition)
        {
            if (definitions.TryGetValue(definition, out var cached))
                return Result.Ok(cached);

            var result = @base.LoadDefinition(definition);

            if (result.IsUnsupported())
                result = Definitions.Load(definition);

            if (result.Ok(out var loaded))
                definitions[definition] = loaded;

            return result;
        }

        public IResult LoadInstance(HIR.Instance instance)
        {
            var result = @base.LoadInstance(instance);

            if (result.IsUnsupported())
                result = Instances.Load(instance);

            return result;
        }

        public IResult LoadType(HIR.Type type)
        {
            var result = @base.LoadType(type);

            if (result.IsUnsupported())
                result = Types.Load(type);

            return result;
        }
    }
}
