namespace ZSharp.Resolver
{
    public static partial class Resolver
    {
        public static RModule Resolve(Module module)
            => new(module.Name)
            {
                Content = new(new(module.Body.Select(Resolve)))
            };
    }
}
