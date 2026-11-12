namespace ZSharp.Compiler
{
    public delegate IResult LoadDefinition(HIR.Definition definition);

    public delegate IResult LoadInstance(HIR.Instance instance);

    public delegate IResult LoadType(HIR.Type type);

    public class HIRLoader
    {
        public LoadDefinition LoadDefinition { get; set; } = definition => Unsupported("definition", definition);

        public LoadInstance LoadInstance { get; set; } = instance => Unsupported("instance", instance);

        public LoadType LoadType { get; set; } = type => Unsupported("type", type);

        public HIRLoader Clone() => (HIRLoader)MemberwiseClone();

        private static IResult Unsupported(string kind, object node)
            => Result.Error(new UnsupportedError($"Can't load HIR {kind} of type {node.GetType().Name}."));
    }
}
