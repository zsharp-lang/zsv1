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
        private readonly Dictionary<HIR.Identity, CompilerObject> identities
            = new(ReferenceEqualityComparer.Instance);

        private HIRLoader @base = null!;

        public LoaderMap<HIR.Definition> Definitions { get; } = new();

        /// <summary>
        /// Loaders for something kept by identity that isn't a definition, such as an
        /// import. A definition is one too, but keeps its own cache and its own map,
        /// <see cref="Definitions"/>, since what already calls <c>LoadDefinition</c> by
        /// name must keep working.
        /// </summary>
        public LoaderMap<HIR.Identity> Identities { get; } = new();

        public LoaderMap<HIR.Instance> Instances { get; } = new();

        public LoaderMap<HIR.Type> Types { get; } = new();

        public void Apply()
        {
            @base = loader.Clone();

            loader.LoadDefinition = LoadDefinition;
            loader.LoadIdentity = LoadIdentity;
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

        public IResult LoadIdentity(HIR.Identity identity)
        {
            if (identities.TryGetValue(identity, out var cached))
                return Result.Ok(cached);

            var result = @base.LoadIdentity(identity);

            if (result.IsUnsupported())
                result = Identities.Load(identity);

            if (result.Ok(out var loaded))
                identities[identity] = loaded;

            return result;
        }

        public IResult LoadInstance(HIR.Instance instance)
        {
            var result = @base.LoadInstance(instance);

            // An Identity that isn't a Definition, such as an Import's member, has no
            // place of its own among the ordinary instance loaders below: whatever loads
            // it is reached, and kept, through LoadIdentity, wherever it is referred to
            // from, not only by its own Get.
            if (result.IsUnsupported() && instance is HIR.Identity identity and not HIR.Definition)
                result = LoadIdentity(identity);

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
