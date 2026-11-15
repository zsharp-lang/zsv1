namespace ZSharp.Compiler
{
    public delegate IResult LoadDefinition(HIR.Definition definition);

    public delegate IResult LoadIdentity(HIR.Identity identity);

    public delegate IResult LoadInstance(HIR.Instance instance);

    public delegate IResult LoadType(HIR.Type type);

    public class HIRLoader
    {
        public LoadDefinition LoadDefinition { get; set; } = definition => Unsupported("definition", definition);

        /// <summary>
        /// Loads something kept by identity that isn't necessarily a definition, such as
        /// an import. A definition is one too; <see cref="LoadDefinition"/> stays its own
        /// entrypoint, for what already calls it by name.
        /// </summary>
        public LoadIdentity LoadIdentity { get; set; } = identity => Unsupported("identity", identity);

        public LoadInstance LoadInstance { get; set; } = instance => Unsupported("instance", instance);

        public LoadType LoadType { get; set; } = type => Unsupported("type", type);

        public HIRLoader Clone() => (HIRLoader)MemberwiseClone();

        private static IResult Unsupported(string kind, object node)
            => Result.Error(new UnsupportedError($"Can't load HIR {kind} of type {node.GetType().Name}."));
    }
}
