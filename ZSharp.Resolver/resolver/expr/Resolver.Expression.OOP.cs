namespace ZSharp.Resolver
{
    public static partial class Resolver
    {
        public static ROOPDefinition Resolve(OOPDefinition oop)
            => new(oop.Type, oop.Name)
            {
                Bases = oop.Bases?.Select(Resolve).ToList(),
                Content = oop.Content is null ? null : Resolve(oop.Content),
            };
    }
}
